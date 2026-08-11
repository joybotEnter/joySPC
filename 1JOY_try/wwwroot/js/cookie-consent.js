

(function () {
    "use strict";

    var STORAGE_KEY = "corpCookieConsent"; // "accepted" | "declined"

    function initCookieBanner() {
        var banner = document.getElementById("corpCookieBanner");
        var acceptBtn = document.getElementById("corpCookieAccept");
        var declineBtn = document.getElementById("corpCookieDecline");

        if (!banner || !acceptBtn || !declineBtn) return;

        var existing;
        try {
            existing = window.localStorage.getItem(STORAGE_KEY);
        } catch (e) {
            existing = null; // localStorage unavailable (e.g. privacy mode) - just show the banner each visit
        }

        if (!existing) {
            banner.classList.add("show");
        }

        function setChoice(value) {
            try {
                window.localStorage.setItem(STORAGE_KEY, value);
            } catch (e) {
                // ignore if storage isn't available
            }
            banner.classList.remove("show");
        }

        acceptBtn.addEventListener("click", function () { setChoice("accepted"); });
        declineBtn.addEventListener("click", function () { setChoice("declined"); });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initCookieBanner);
    } else {
        initCookieBanner();
    }
})();