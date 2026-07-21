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

    const menuSections = Array.from(document.querySelectorAll("[data-menu-section]"));
    const storageKey = "rss-sidebar-open-sections";
    const storedOpenSections = (() => {
        try {
            return new Set(JSON.parse(localStorage.getItem(storageKey) || "[]"));
        } catch {
            return new Set();
        }
    })();

    const persistOpenSections = () => {
        const openIndexes = menuSections
            .map((section, index) => section.classList.contains("is-open") ? index : null)
            .filter((index) => index !== null);

        localStorage.setItem(storageKey, JSON.stringify(openIndexes));
    };

    menuSections.forEach((section, index) => {
        const toggleButton = section.querySelector(".tech-nav-section-toggle");
        const hasActiveItem = section.querySelector(".tech-nav-link.active");
        const shouldRestoreOpen = storedOpenSections.has(index);
        const shouldOpen = Boolean(hasActiveItem || shouldRestoreOpen);

        section.classList.toggle("is-open", shouldOpen);
        toggleButton?.setAttribute("aria-expanded", shouldOpen ? "true" : "false");

        toggleButton?.addEventListener("click", () => {
            const isOpen = !section.classList.contains("is-open");
            section.classList.toggle("is-open", isOpen);
            toggleButton.setAttribute("aria-expanded", isOpen ? "true" : "false");
            persistOpenSections();
        });
    });

    document.querySelectorAll(".tech-sidebar .tech-nav-link").forEach((link) => {
        link.addEventListener("click", () => {
            if (window.matchMedia("(max-width: 992px)").matches) {
                setSidebarState(false);
            }
        });
    });
})();
