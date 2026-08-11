// Site-wide FAQ chatbot for visitors.
// Edit the constants and FAQ_RULES below - no server or API key required.

(function () {
    "use strict";

    // ---- BASIC COMPANY INFO - edit to match reality ----
    var COMPANY_NAME = "State Properties Corporation";
    var CONTACT_EMAIL = "careers@yourcompany.com"; // TODO: replace with real HR email
    var CONTACT_PHONE = "(02) 000-0000"; // TODO: replace with real phone number
    var APPLY_URL = "#"; // TODO: replace with a real application form / careers page link
    var CURRENT_OPENINGS_TEXT = "We're currently accepting applications for roles across our real estate development, finance, and operations teams. Specific vacancies are posted as they open."; // TODO: keep updated

    // ---- SOCIAL MEDIA & RELATED WEBSITE LINKS ----
    // Keep these in sync with the "Connect With Us" links on the Home page.
    var SOCIAL_LINKS = [
        { label: "SIHI (State Investment House, Inc.)", url: "https://stateinvestment.com.ph/" },       // TODO: real URL
        { label: "SPC (State Properties Corporation) website", url: "https://statepropertiescorp.com/" }, // TODO: real URL
        { label: "SPC on Facebook", url: "https://www.facebook.com/statepropertiesofficial" },                            // TODO: real URL
        { label: "Versailles website", url: "https://versailles.com.ph/" },                        // TODO: real URL
        { label: "Versailles on Instagram", url: "https://www.instagram.com/versaillesalabangofficial/" },                   // TODO: real URL
        { label: "Versailles on Facebook", url: "https://www.facebook.com/versaillesalabangofficial" },                    // TODO: real URL
        { label: "VTP on Facebook", url: "https://www.facebook.com/versaillestownplaza" }                            // TODO: real URL
    ];

    function socialLinksHtml() {
        var items = SOCIAL_LINKS.map(function (link) {
            var href = link.url && link.url !== "#" ? link.url : null;
            if (href) {
                return "<li><a href=\"" + href + "\" target=\"_blank\" rel=\"noopener\">" + link.label + "</a></li>";
            }
            return "<li>" + link.label + " (link coming soon)</li>";
        });
        return "Here's where you can find us online:<ul style=\"margin:6px 0 0 18px; padding:0;\">" + items.join("") + "</ul>";
    }

    // ---- FULL COMPANY HISTORY (from the Home page timeline) ----
    var HISTORY_TIMELINE = [
        "1964 — Started operation.",
        "1974 — Granted a quasi-banking authority by the Bangko Sentral ng Pilipinas (BSP).",
        "1975 — Ventured into various real estate developments with condominium projects in Manila, Makati, Greenhills, Iloilo, and Bacolod.",
        "1982 — Became the first investment house to be granted a Trust License.",
        "2001 — Set up a joint venture, Steag State Power Inc., with STEAG AG of Germany, to build and operate a ₱13.5 billion 210-megawatt power plant in Cagayan de Oro City, with EPC awarded to Kawasaki-Nissho Iwai — a major development in Southern Philippines.",
        "2005 — Through its wholly-owned subsidiary, State Properties Corporation, ventured into the development of Versailles, a high-end residential/commercial subdivision in Alabang — a 78-hectare property valued at ₱15.75 billion."
    ];

    function historyHtml() {
        var items = HISTORY_TIMELINE.map(function (line) {
            return "<li>" + line + "</li>";
        });
        return "Here's our company history:<ul style=\"margin:6px 0 0 18px; padding:0;\">" + items.join("") + "</ul>";
    }

    var SPC_PROFILE = "State Properties Corporation is a wholly owned real estate arm of State Investment Trust, Inc., originally organized in 1975 under the name Peltan, Inc., and renamed State Properties Corporation in 1995. The company is currently developing its 78.6-hectare prime property into a high-end subdivision called Versailles, strategically located adjacent to Ayala Alabang Village. It caters to the country's upper income strata and is also marketed to Filipinos based in the United States and Europe. Other group-owned assets along EDSA in Metropolitan Manila are also lined up for future development.";

    var SIHI_PROFILE = "State Investment House, Inc. (SIHI) is an investment house providing an array of financial products and services through its different licenses and special authorities granted by the Bangko Sentral ng Pilipinas (BSP) and the Securities and Exchange Commission (SEC).";

    var VERSAILLES_INFO = "Versailles is our flagship 78.6-hectare high-end residential/commercial subdivision, strategically located adjacent to Ayala Alabang Village. Development began in 2005 through our subsidiary State Properties Corporation, and the property is valued at approximately ₱15.75 billion.";

    var NOTABLE_PROJECTS = "Our notable projects include Versailles (our 78.6-hectare flagship subdivision), Steag State Power Inc. (a ₱13.5 billion power plant joint venture in Cagayan de Oro City), our early Metro Manila condominium developments, and additional group-owned assets along EDSA lined up for future development.";

    var LEADERSHIP_NOTE = "Leadership and management team details are on our Home page under 'Leadership Team.' For specific inquiries about our executives, please contact us at " + CONTACT_EMAIL + ".";

    function valuesHtml() {
        return "Our core values are:<ul style=\"margin:6px 0 0 18px; padding:0;\">"
            + "<li><strong>Concern for Honor</strong> — The trust of the client is a gift that we cherish and protect with great care. We honor our commitment.</li>"
            + "<li><strong>Respect for Tradition</strong> — In a world of sweeping changes, we find stability in the virtues earned from tradition such as integrity, hard work, discipline and cooperation.</li>"
            + "<li><strong>Loyalty to Service</strong> — Business is reciprocity. Patronage deserves reciprocal service in terms of utmost professionalism, courtesy and dependability.</li>"
            + "<li><strong>Commitment to Excellence</strong> — We premise our institutional relevance on the continuous refinement and upgrading of our knowledge, skills, capabilities and services.</li>"
            + "</ul>";
    }

    // ---- FAQ RULES ----
    // Order matters: first matching rule wins. More specific rules are placed above general ones.
    var FAQ_RULES = [
        // Social media / website links
        {
            keywords: ["social media", "social accounts", "socials", "follow you", "follow us", "your website", "your web", "official website", "your links", "all your links", "facebook page", "instagram page", "ig account", "fb page"],
            html: socialLinksHtml()
        },

        // Company history
        {
            keywords: ["history", "founded", "when did you start", "when were you started", "company background", "timeline", "how long have you been"],
            html: historyHtml()
        },

        // Corporate profiles
        {
            keywords: ["what is spc", "about spc", "state properties corporation"],
            answer: SPC_PROFILE
        },
        {
            keywords: ["sihi", "state investment house"],
            answer: SIHI_PROFILE
        },
        {
            keywords: ["versailles", "the subdivision", "hectares", "alabang"],
            answer: VERSAILLES_INFO
        },
        {
            keywords: ["values", "core values", "what do you stand for", "company culture"],
            html: valuesHtml()
        },
        {
            keywords: ["projects", "portfolio", "notable projects", "steag", "power plant", "condominium"],
            answer: NOTABLE_PROJECTS
        },
        {
            keywords: ["leadership", "management team", "ceo", "chairman", "executives", "who runs"],
            answer: LEADERSHIP_NOTE
        },
        {
            keywords: ["about", "what do you do", "what does the company do", "who are you", "company profile"],
            answer: COMPANY_NAME + " is the real estate development arm of State Investment Trust, Inc. " + SPC_PROFILE
        },

        // Hiring / careers
        {
            keywords: ["hiring", "hire", "job opening", "openings", "recruiting", "recruitment"],
            answer: "Yes, " + COMPANY_NAME + " hires on an ongoing basis. " + CURRENT_OPENINGS_TEXT
        },
        {
            keywords: ["vacant", "vacancy", "vacancies", "position available", "positions available", "what position", "what job"],
            answer: CURRENT_OPENINGS_TEXT + " For the most current list, please reach out to us directly at " + CONTACT_EMAIL + "."
        },
        {
            keywords: ["apply", "application", "how to apply", "how do i apply", "submit resume", "submit my resume", "send resume", "send my resume", "cv"],
            answer: "To apply, send your resume/CV to " + CONTACT_EMAIL + (APPLY_URL !== "#" ? ", or use our online application form here: " + APPLY_URL : "") + ". Our HR team reviews every application and will reach out if there's a fit."
        },
        {
            keywords: ["salary", "pay", "compensation", "how much"],
            answer: "Compensation depends on the role and experience level. Specific details are discussed during the interview process - feel free to apply and our HR team will follow up."
        },
        {
            keywords: ["requirement", "qualifications", "who can apply", "eligib"],
            answer: "Requirements vary by role. In general we look for relevant experience or education in the field, strong communication skills, and alignment with our company values. Send your resume to " + CONTACT_EMAIL + " and we'll match you to suitable openings."
        },
        {
            keywords: ["intern", "internship", "ojt", "on the job training"],
            answer: "We periodically welcome interns/OJT students. Email " + CONTACT_EMAIL + " with your school, program, and required internship dates."
        },

        // Location / contact
        {
            keywords: ["located", "location", "address", "where are you", "where r u", "where is your office", "office address"],
            answer: "Our main development, Versailles, is strategically located adjacent to Ayala Alabang Village. Contact us at " + CONTACT_EMAIL + " for the exact office address and directions."
        },
        {
            keywords: ["contact", "phone", "email", "reach you", "get in touch", "call you"],
            answer: "You can reach us by email at " + CONTACT_EMAIL + " or by phone at " + CONTACT_PHONE + "."
        },

        // Small talk
        {
            keywords: ["hello", "hi", "hey", "good morning", "good afternoon"],
            answer: "Hello! I can help with questions about hiring, open positions, how to apply, our history, and where to find us online. What would you like to know?"
        },
        {
            keywords: ["thank", "thanks"],
            answer: "You're welcome! Let me know if you have any other questions."
        }
    ];

    var FALLBACK_ANSWER = "I'm not sure about that one. For anything I can't answer, please email " + CONTACT_EMAIL + " and our team will help directly.";

    function findRule(userText) {
        var text = userText.toLowerCase();
        for (var i = 0; i < FAQ_RULES.length; i++) {
            var rule = FAQ_RULES[i];
            for (var j = 0; j < rule.keywords.length; j++) {
                if (text.indexOf(rule.keywords[j]) !== -1) {
                    return rule;
                }
            }
        }
        return null;
    }

    function scrollToBottom(el) {
        el.scrollTop = el.scrollHeight;
    }

    function appendMessage(logEl, content, sender, isHtml) {
        var msg = document.createElement("div");
        msg.className = "corp-chat-msg corp-chat-msg-" + sender;
        if (isHtml) {
            msg.innerHTML = content;
        } else {
            msg.textContent = content;
        }
        logEl.appendChild(msg);
        scrollToBottom(logEl);
    }

    function initChatbot() {
        var toggleBtn = document.getElementById("corpChatToggle");
        var panel = document.getElementById("corpChatPanel");
        var closeBtn = document.getElementById("corpChatClose");
        var form = document.getElementById("corpChatForm");
        var input = document.getElementById("corpChatInput");
        var log = document.getElementById("corpChatLog");

        if (!toggleBtn || !panel || !form || !input || !log) return;

        var greeted = false;

        toggleBtn.addEventListener("click", function () {
            var isOpen = panel.classList.toggle("open");
            toggleBtn.setAttribute("aria-expanded", isOpen ? "true" : "false");
            if (isOpen) {
                input.focus();
                if (!greeted) {
                    greeted = true;
                    appendMessage(log, "Hi! Ask me about hiring, open positions, how to apply, our history, or where to find us online.", "bot", false);
                }
            }
        });

        if (closeBtn) {
            closeBtn.addEventListener("click", function () {
                panel.classList.remove("open");
                toggleBtn.setAttribute("aria-expanded", "false");
            });
        }

        form.addEventListener("submit", function (e) {
            e.preventDefault();
            var text = input.value.trim();
            if (!text) return;

            appendMessage(log, text, "user", false);
            input.value = "";

            var rule = findRule(text);
            setTimeout(function () {
                if (rule && rule.html) {
                    appendMessage(log, rule.html, "bot", true);
                } else if (rule && rule.answer) {
                    appendMessage(log, rule.answer, "bot", false);
                } else {
                    appendMessage(log, FALLBACK_ANSWER, "bot", false);
                }
            }, 300);
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initChatbot);
    } else {
        initChatbot();
    }
})();