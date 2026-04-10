// =============================================================
//  ai-panel.js
//  Same fetch call, markdown parser, typing indicator, and
//  message renderer as the Index page.
//  Open/close uses Tailwind class toggling — no custom CSS needed
//  except the three edge cases in ai-panel.css.
// =============================================================

(function () {
    "use strict";

    // ── DOM refs ───────────────────────────────────────────────
    const tab      = document.getElementById("ai-panel-tab");
    const panel    = document.getElementById("ai-panel");
    const closeBtn = document.getElementById("ai-panel-close");
    const backdrop = document.getElementById("ai-backdrop");
    const msgList  = document.getElementById("ai-panel-messages");
    const input    = document.getElementById("ai-panel-input");
    const sendBtn  = document.getElementById("ai-panel-send");

    if (!tab || !panel) return;

    // ── Open / close — all via Tailwind class toggles ─────────
    function openPanel() {
        // Panel: slide in
        panel.classList.remove("translate-x-full");
        panel.classList.add("translate-x-0");
        // Backdrop: fade in + enable clicks
        backdrop.classList.remove("opacity-0", "pointer-events-none");
        backdrop.classList.add("opacity-100");
        // Tab: hide (compound transform handled by ai-panel.css .is-open)
        tab.classList.add("is-open");
        tab.setAttribute("aria-expanded", "true");
        panel.setAttribute("aria-hidden", "false");
        input.focus();
    }

    function closePanel() {
        panel.classList.add("translate-x-full");
        panel.classList.remove("translate-x-0");
        backdrop.classList.add("opacity-0", "pointer-events-none");
        backdrop.classList.remove("opacity-100");
        tab.classList.remove("is-open");
        tab.setAttribute("aria-expanded", "false");
        panel.setAttribute("aria-hidden", "true");
    }

    tab.addEventListener("click", openPanel);
    closeBtn.addEventListener("click", closePanel);
    backdrop.addEventListener("click", closePanel);

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape" && !panel.classList.contains("translate-x-full")) {
            closePanel();
        }
    });

    input.addEventListener("keydown", function (e) {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });
    sendBtn.addEventListener("click", sendMessage);

    // ── Markdown parser — identical to Index ──────────────────
    function parseMarkdown(text) {
        return text
            .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
            .replace(/(?<!\*)\*(?!\*)(.*?)(?<!\*)\*(?!\*)/g, '<em>$1</em>')
            .replace(
                /\[([^\]]+)\]\((https?:\/\/[^\)]+)\)/g,
                '<a href="$2" target="_blank" rel="noopener noreferrer" ' +
                'class="underline text-[#C6AC8F] hover:text-white transition-colors">$1</a>'
            )
            .replace(/^- (.+)$/gm, '<li class="ml-4 list-disc">$1</li>')
            .replace(/^\d+\. (.+)$/gm, '<li class="ml-4 list-decimal">$1</li>')
            .replace(/(<li[^>]*>[\s\S]*?<\/li>\n?)+/g,
                function (m) { return '<ul class="mt-2 mb-2 space-y-1">' + m + '</ul>'; })
            .replace(/\n/g, '<br />');
    }

    // ── Message renderer — identical to Index ─────────────────
    function appendMessage(text, role) {
        const isUser = role === "user";

        const wrapper = document.createElement("div");
        wrapper.className = "flex items-start gap-3" + (isUser ? " flex-row-reverse" : "");

        const avatar = document.createElement("div");
        avatar.className =
            "mt-1 flex h-7 w-7 shrink-0 items-center justify-center rounded-full " +
            (isUser ? "bg-[var(--tan)]" : "bg-[#22333B]");
        avatar.innerHTML = isUser
            ? `<svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                   fill="none" stroke="currentColor" stroke-width="2"
                   stroke-linecap="round" stroke-linejoin="round" class="text-[#22333B]">
                   <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
                   <circle cx="12" cy="7" r="4"/>
               </svg>`
            : `<svg viewBox="0 0 24 24" height="14" width="14" fill="currentColor" class="text-[var(--tan)]">
                   <path d="M9.107 5.448c.598-1.75 3.016-1.803 3.725-.159l.06.16l.807 2.36a4 4 0 0 0 2.276 2.411l.217.081l2.36.806c1.75.598 1.803 3.016.16 3.725l-.16.06l-2.36.807a4 4 0 0 0-2.412 2.276l-.081.216l-.806 2.361c-.598 1.75-3.016 1.803-3.724.16l-.062-.16l-.806-2.36a4 4 0 0 0-2.276-2.412l-.216-.081l-2.36-.806c-1.751-.598-1.804-3.016-.16-3.724l.16-.062l2.36-.806A4 4 0 0 0 8.22 8.025l.081-.216z"/>
               </svg>`;

        const bubble = document.createElement("div");
        bubble.className =
            "max-w-[80%] rounded-2xl px-4 py-3 text-sm shadow-sm leading-relaxed " +
            (isUser
                ? "rounded-tr-none bg-[var(--tan)] text-[var(--jet)]"
                : "rounded-tl-none bg-[#22333B] text-[#EAE0D5]");

        if (isUser) {
            bubble.textContent = text;
        } else {
            bubble.innerHTML = parseMarkdown(text);
        }

        wrapper.appendChild(avatar);
        wrapper.appendChild(bubble);
        msgList.appendChild(wrapper);
        msgList.scrollTop = msgList.scrollHeight;
    }

    // ── Typing indicator — identical to Index ─────────────────
    function appendTyping() {
        const wrapper = document.createElement("div");
        wrapper.id = "ai-panel-typing";
        wrapper.className = "flex items-start gap-3";
        wrapper.innerHTML = `
            <div class="mt-1 flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-[#22333B]">
                <svg viewBox="0 0 24 24" height="14" width="14" fill="currentColor" class="text-[var(--tan)]">
                    <path d="M9.107 5.448c.598-1.75 3.016-1.803 3.725-.159l.06.16l.807 2.36a4 4 0 0 0 2.276 2.411l.217.081l2.36.806c1.75.598 1.803 3.016.16 3.725l-.16.06l-2.36.807a4 4 0 0 0-2.412 2.276l-.081.216l-.806 2.361c-.598 1.75-3.016 1.803-3.724.16l-.062-.16l-.806-2.36a4 4 0 0 0-2.276-2.412l-.216-.081l-2.36-.806c-1.751-.598-1.804-3.016-.16-3.724l.16-.062l2.36-.806A4 4 0 0 0 8.22 8.025l.081-.216z"/>
                </svg>
            </div>
            <div class="rounded-2xl rounded-tl-none bg-[#22333B] px-4 py-3 shadow-sm">
                <div class="flex gap-1">
                    <span class="h-2 w-2 animate-bounce rounded-full bg-[#C6AC8F]" style="animation-delay:0ms"></span>
                    <span class="h-2 w-2 animate-bounce rounded-full bg-[#C6AC8F]" style="animation-delay:150ms"></span>
                    <span class="h-2 w-2 animate-bounce rounded-full bg-[#C6AC8F]" style="animation-delay:300ms"></span>
                </div>
            </div>`;
        msgList.appendChild(wrapper);
        msgList.scrollTop = msgList.scrollHeight;
    }

    function removeTyping() {
        document.getElementById("ai-panel-typing")?.remove();
    }

    // ── Send — same endpoint as Index: POST /Home/AskAI ───────
    async function sendMessage() {
        const question = input.value.trim();
        if (!question) return;

        appendMessage(question, "user");
        input.value = "";
        sendBtn.disabled = true;
        appendTyping();

        try {
            const response = await fetch("/Home/AskAI", {
                method:  "POST",
                headers: { "Content-Type": "application/json" },
                body:    JSON.stringify({ question })
            });
            const data = await response.json();
            removeTyping();
            appendMessage(data.answer ?? "Sorry, I couldn't get a response.", "ai");
        } catch (err) {
            removeTyping();
            appendMessage("Something went wrong. Please try again.", "ai");
        } finally {
            sendBtn.disabled = false;
            input.focus();
        }
    }

})();
