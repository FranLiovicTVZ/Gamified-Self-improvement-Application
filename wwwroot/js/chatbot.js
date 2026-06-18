(function () {
    'use strict';

    const SUGGESTED = [
        'Kako meditiram kao početnik?',
        'Daj mi plan vježbanja za tjedan',
        'Tehnika dubokog disanja za opuštanje',
        'Koji mišići su najvažniji za trenirati?'
    ];

    let history = [];
    let isOpen = false;
    let isLoading = false;

    function init() {
        const toggleBtn = document.getElementById('ai-chat-toggle');
        const panel = document.getElementById('ai-chat-panel');
        const closeBtn = document.getElementById('ai-chat-close');
        const messagesEl = document.getElementById('ai-chat-messages');
        const input = document.getElementById('ai-chat-input');
        const sendBtn = document.getElementById('ai-chat-send');
        const suggestedEl = document.getElementById('ai-chat-suggested');

        if (!toggleBtn) return;

        toggleBtn.addEventListener('click', () => toggle(panel, toggleBtn));
        closeBtn.addEventListener('click', () => close(panel, toggleBtn));

        sendBtn.addEventListener('click', () => send(input, messagesEl));
        input.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                send(input, messagesEl);
            }
        });

        input.addEventListener('input', () => {
            input.style.height = 'auto';
            input.style.height = Math.min(input.scrollHeight, 100) + 'px';
        });

        SUGGESTED.forEach(q => {
            const btn = document.createElement('button');
            btn.className = 'ai-suggested-btn';
            btn.textContent = q;
            btn.addEventListener('click', () => {
                input.value = q;
                input.dispatchEvent(new Event('input'));
                send(input, messagesEl);
            });
            suggestedEl.appendChild(btn);
        });

        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && isOpen) close(panel, toggleBtn);
        });
    }

    function toggle(panel, toggleBtn) {
        isOpen ? close(panel, toggleBtn) : open(panel, toggleBtn);
    }

    function open(panel, toggleBtn) {
        isOpen = true;
        panel.classList.add('open');
        toggleBtn.setAttribute('aria-expanded', 'true');
        toggleBtn.innerHTML = '<i class="bi bi-x-lg"></i>';
        setTimeout(() => document.getElementById('ai-chat-input')?.focus(), 300);
    }

    function close(panel, toggleBtn) {
        isOpen = false;
        panel.classList.remove('open');
        toggleBtn.setAttribute('aria-expanded', 'false');
        toggleBtn.innerHTML = '<i class="bi bi-chat-heart-fill"></i>';
    }

    async function send(input, messagesEl) {
        const text = input.value.trim();
        if (!text || isLoading) return;

        input.value = '';
        input.style.height = 'auto';

        const suggestedEl = document.getElementById('ai-chat-suggested');
        if (suggestedEl) suggestedEl.style.display = 'none';

        appendMsg(messagesEl, 'user', text);

        isLoading = true;
        const loadingId = appendLoading(messagesEl);

        try {
            const res = await fetch('/api/chat', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: text, history })
            });

            const data = await res.json();
            removeEl(loadingId);

            if (res.ok) {
                const reply = data.reply || 'Nema odgovora.';
                appendMsg(messagesEl, 'bot', reply);
                history.push({ role: 'user', text });
                history.push({ role: 'model', text: reply });
                if (history.length > 20) history = history.slice(-20);
            } else {
                appendMsg(messagesEl, 'bot', data.error || 'Greška. Pokušajte ponovno.');
            }
        } catch {
            removeEl(loadingId);
            appendMsg(messagesEl, 'bot', 'Greška mreže. Provjerite internetsku vezu.');
        } finally {
            isLoading = false;
        }
    }

    function appendMsg(container, role, text) {
        const div = document.createElement('div');
        div.className = 'ai-msg ai-msg-' + role;
        const bubble = document.createElement('div');
        bubble.className = 'ai-bubble';
        bubble.innerHTML = formatText(text);
        div.appendChild(bubble);
        container.appendChild(div);
        container.scrollTop = container.scrollHeight;
    }

    function appendLoading(container) {
        const id = 'ai-loading-' + Date.now();
        const div = document.createElement('div');
        div.className = 'ai-msg ai-msg-bot';
        div.id = id;
        div.innerHTML = '<div class="ai-bubble ai-loading"><span></span><span></span><span></span></div>';
        container.appendChild(div);
        container.scrollTop = container.scrollHeight;
        return id;
    }

    function removeEl(id) {
        document.getElementById(id)?.remove();
    }

    function formatText(raw) {
        return raw
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
            .replace(/\*(.*?)\*/g, '<em>$1</em>')
            .replace(/\n/g, '<br>');
    }

    document.addEventListener('DOMContentLoaded', init);
})();
