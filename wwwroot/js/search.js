(function () {
    const input = document.getElementById('globalSearch');
    const dropdown = document.getElementById('searchDropdown');
    if (!input || !dropdown) return;

    let debounceTimer = null;

    const CATEGORY_ORDER = ['Stranica', 'Aktivnost', 'Korisnik', 'Knjiga'];

    input.addEventListener('input', () => {
        clearTimeout(debounceTimer);
        const q = input.value.trim();
        if (q.length < 2) { hideDropdown(); return; }
        debounceTimer = setTimeout(() => fetchResults(q), 300);
    });

    input.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') { hideDropdown(); input.blur(); return; }
        if (e.key === 'Enter') {
            e.preventDefault();
            const q = input.value.trim();
            if (q.length >= 2) window.location.href = `/pretraga?q=${encodeURIComponent(q)}`;
        }
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            const first = dropdown.querySelector('.search-item');
            if (first) first.focus();
        }
    });

    document.addEventListener('click', (e) => {
        if (!input.contains(e.target) && !dropdown.contains(e.target)) hideDropdown();
    });

    async function fetchResults(q) {
        try {
            const res = await fetch(`/api/search?q=${encodeURIComponent(q)}&limit=5`);
            if (!res.ok) { hideDropdown(); return; }
            const data = await res.json();
            renderDropdown(data);
        } catch { hideDropdown(); }
    }

    function renderDropdown(data) {
        if (!data.results || data.results.length === 0) {
            dropdown.innerHTML = '<div class="search-empty">Nema rezultata</div>';
            showDropdown();
            return;
        }

        const grouped = {};
        for (const item of data.results) {
            if (!grouped[item.category]) grouped[item.category] = [];
            grouped[item.category].push(item);
        }

        let html = '';
        for (const cat of CATEGORY_ORDER) {
            if (!grouped[cat]) continue;
            html += `<div class="search-category-header">${cat}</div>`;
            for (const item of grouped[cat]) {
                html += `<a href="${escHtml(item.url)}" class="search-item" tabindex="0">
                    <i class="bi ${escHtml(item.icon)} search-item-icon"></i>
                    <div class="search-item-text">
                        <div class="search-item-title">${escHtml(item.title)}</div>
                        <div class="search-item-subtitle">${escHtml(item.subtitle)}</div>
                    </div>
                </a>`;
            }
        }

        const q = data.query;
        html += `<a href="/pretraga?q=${encodeURIComponent(q)}" class="search-all-link">
            Prikaži sve rezultate za "<strong>${escHtml(q)}</strong>"
        </a>`;

        dropdown.innerHTML = html;

        // Keyboard navigation within dropdown
        dropdown.querySelectorAll('.search-item, .search-all-link').forEach((el, idx, arr) => {
            el.addEventListener('keydown', (e) => {
                if (e.key === 'ArrowDown') { e.preventDefault(); arr[idx + 1]?.focus(); }
                if (e.key === 'ArrowUp')   { e.preventDefault(); idx === 0 ? input.focus() : arr[idx - 1]?.focus(); }
                if (e.key === 'Escape')    { hideDropdown(); input.focus(); }
            });
        });

        showDropdown();
    }

    function showDropdown() { dropdown.classList.remove('d-none'); }
    function hideDropdown() { dropdown.classList.add('d-none'); dropdown.innerHTML = ''; }

    function escHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }
})();
