(() => {
    const shell = document.getElementById("techAppShell");
    const toggle = document.querySelector(".tech-menu-toggle");
    const backdrop = document.querySelector("[data-sidebar-close]");

    if (!shell || !toggle) {
        return;
    }

    const setSidebarState = (isOpen) => {
        shell.classList.toggle("sidebar-open", isOpen);
        toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
    };

    toggle.addEventListener("click", () => {
        setSidebarState(!shell.classList.contains("sidebar-open"));
    });

    backdrop?.addEventListener("click", () => {
        setSidebarState(false);
    });

    window.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            setSidebarState(false);
        }
    });
})();
