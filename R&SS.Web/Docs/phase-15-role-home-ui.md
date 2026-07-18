# Phase 15 MVC Role Home UI Design

This design targets the MVC layer only. All future live data should come through typed `HttpClient` API clients that call `R&SS.API`; MVC views and controllers must not call DAL repositories or DbContext directly.

## Shared Layout Direction

The MVC home experience should use a role-aware application shell:

- Top bar: brand, current role, user menu, logout.
- Left navigation on desktop; collapsible navigation on mobile.
- Main content begins with role title, short status summary, and primary actions.
- Reusable components: action tiles, queue/table preview, metric summary, alert strip, and compact activity list.
- Empty/error states: each widget should have a friendly empty state and a retry action for API failures.

The visual style should stay operational and scan-friendly: restrained color, clear action hierarchy, dense but readable tables, and no marketing-style landing page.

## Customer Home

Primary goal: let customers request service or buy products quickly.

Layout:

- Header: "Customer Home" with account status and most recent ticket/order summary.
- Primary action row:
  - Create Service Ticket
  - Shop Products
- Active service panel:
  - Recent ticket number, device/product, status, last update.
  - Button: View My Tickets.
- Shopping panel:
  - Featured product categories or recommended products.
  - Cart summary and button: View Cart.
- Notifications panel:
  - Repair updates, payment due, delivery OTP, feedback request.

Recommended first screen priority:

1. Create Service Ticket.
2. Shop Products.
3. Track current tickets/orders.

## Receptionist Home

Primary goal: create service requests and manage customer profiles at the front desk.

Layout:

- Header: "Reception Desk" with today's intake count and pending handoff count.
- Primary action row:
  - New Ticket
  - Find Customer
  - Register Customer
- Intake queue:
  - Customer, device/product, request type, priority, created time, status.
  - Row action: Open.
- Customer lookup panel:
  - Search by name, phone, email, or customer code.
  - Recent customers edited today.
- Operational notes:
  - Tickets missing technician assignment.
  - Tickets awaiting customer confirmation.

Recommended first screen priority:

1. New Ticket.
2. Customer search/edit.
3. Queue triage.

## Technician Home

Primary goal: show assigned work and make progress updates fast.

Layout:

- Header: "Technician Workbench" with assigned, in-progress, and blocked counts.
- Primary action row:
  - View Assigned Tickets
  - Update Progress
- Assigned ticket list:
  - Ticket, customer/device, priority, current status, due date/SLA, last note.
  - Row actions: Update Status, Add Note.
- Status update panel:
  - Quick status selector: Diagnosing, Waiting Parts, Repairing, Ready for Pickup.
  - Recent progress history.
- Parts/notes panel:
  - Used components and technician notes preview.

Recommended first screen priority:

1. Assigned tickets.
2. Update status/progress.
3. Review blocked or waiting-parts work.

## Manager Home

Primary goal: operational oversight and staff assignment.

Layout:

- Header: "Manager Overview" with date range selector.
- Metrics row:
  - Open Tickets
  - Awaiting Assignment
  - Orders Today
  - Revenue Today
- Primary action row:
  - Assign Staff
  - View Reports
  - Review Inventory
- Operations panels:
  - Ticket backlog by status.
  - Technician workload.
  - Recent sales and service activity.
- Report shortcuts:
  - Revenue report.
  - Inventory statistics.
  - Activity history.

Recommended first screen priority:

1. Backlog and workload.
2. Staff assignment.
3. Reports.

## Administrator Home

Primary goal: system control, user/role management, and the same operational overview as Manager.

Layout:

- Header: "Administrator Console" with system health indicators.
- Metrics row:
  - Active Users
  - Open Tickets
  - Pending Role Changes
  - System Alerts
- Primary action row:
  - Manage Users
  - Manage Roles
  - System Configuration
  - View Reports
- Admin panels:
  - User management queue.
  - Role and permission shortcuts.
  - System activity log.
- Operations panels:
  - Same core overview pattern as Manager, but lower priority than access/system controls.

Recommended first screen priority:

1. User and role management.
2. System alerts/activity.
3. Operational overview.

## Navigation Plan

Common items for all signed-in users:

- Home
- Notifications
- Profile
- Logout

Customer:

- My Tickets
- New Service Ticket
- Products
- Cart
- My Orders
- Feedback

Receptionist:

- Tickets
- New Ticket
- Customers
- Service Queue

Technician:

- Assigned Tickets
- Progress Updates
- Repair History

Manager:

- Dashboard
- Tickets
- Staff Assignment
- Reports
- Inventory
- Activity History

Administrator:

- Dashboard
- Users
- Roles & Permissions
- Tickets
- Staff Assignment
- Reports
- Inventory
- System Configuration
- Activity Log

## MVC Implementation Notes For Later

- Add typed API clients, for example `IAuthApiClient`, `ITicketApiClient`, `ICustomerApiClient`, `IProductApiClient`, `IReportApiClient`, and `IAdminApiClient`.
- Store access token and role claims in session or secure cookies after login.
- Add a delegating handler that attaches the bearer token to API requests.
- Centralize `401` and `403` handling:
  - `401`: clear session/cookie and redirect to login with return URL.
  - `403`: show an access denied page with a role-aware return action.
- Build a role menu provider in MVC that reads role claims and returns allowed nav items.
- Keep role home actions as MVC links first; load live counts from API later when API testing begins.
- Do not inject DAL repositories, `IUnitOfWork`, or `AppDbContext` into MVC controllers.

