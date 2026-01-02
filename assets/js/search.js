// Поиск по сайту
const searchIndex = {};

// Загружаем индекс
fetch('/assets/search-index.json')
    .then(response => response.json())
    .then(data => {
        // Теперь у нас только классы и свойства
        searchIndex.classes = data.classes || [];
        searchIndex.properties = data.properties || [];

        // Объединяем все элементы для поиска
        searchIndex.all = [
            ...(data.classes || []),
            ...(data.properties || [])
        ];
    })
    .catch(error => console.error('Error loading search index:', error));

// Функция поиска
function performSearch(query) {
    if (!searchIndex.all || searchIndex.all.length === 0) {
        return [];
    }

    const lowerQuery = query.toLowerCase().trim();
    if (lowerQuery.length < 2) {
        return [];
    }

    return searchIndex.all.filter(item => {
        return (
            item.id.toLowerCase().includes(lowerQuery) ||
            item.name.toLowerCase().includes(lowerQuery) ||
            (item.description && item.description.toLowerCase().includes(lowerQuery)) ||
            (item.domain && item.domain.toLowerCase().includes(lowerQuery)) ||
            (item.range && item.range.toLowerCase().includes(lowerQuery))
        );
    });
}

// === МАППИНГ СТЕРЕОТИПА В CSS-КЛАСС ===
function getBadgeClass(item) {
    // Свойства — всегда зелёные
    if (item.type && item.type.toLowerCase() === 'property') {
        return 'badge-property';
    }

    const stereotype = (item.stereotype || item.Stereotype || '').toLowerCase();

    switch (stereotype) {
        case 'class':
            return 'badge-class';
        case 'enum':
            return 'badge-enum';
        case 'datatype':
            return 'badge-datatype';
        case 'primitive':
            return 'badge-primitive';
        case 'all':
            return 'bg-secondary';
        default:
            return 'bg-secondary';
    }
}

// В обработчике DOMContentLoaded
document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.getElementById('searchInput');
    const searchResults = document.getElementById('searchResults');

    if (!searchInput || !searchResults) return;

    let searchTimeout;

    searchInput.addEventListener('input', function (e) {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            const query = e.target.value;
            const results = performSearch(query);
            updateSearchResults(results, query);
        }, 300);
    });

    // Закрытие при клике вне области
    document.addEventListener('click', function (e) {
        if (!searchResults.contains(e.target) && e.target !== searchInput) {
            searchResults.classList.add('d-none');
        }
    });

    // Закрытие при нажатии Escape
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            searchResults.classList.add('d-none');
        }
    });
});

function updateSearchResults(results, query) {
    const searchResults = document.getElementById('searchResults');
    const searchInput = document.getElementById('searchInput');

    if (!searchResults || !searchInput) return;

    if (results.length === 0 || query.length < 2) {
        searchResults.classList.add('d-none');
        return;
    }

    // Сортируем результаты (оставьте вашу текущую логику сортировки)
    results.sort((a, b) => {
        // ... ваш текущий код сортировки ...
    });

    // Ограничиваем количество результатов
    const displayResults = results.slice(0, 10);

    // Генерируем HTML
    let html = '';

    displayResults.forEach(result => {
        const highlightedName = highlightText(result.name, query);
        const highlightedId = highlightText(result.id, query);
        const badgeClass = getBadgeClass(result);

        html += `
            <a href="${result.url}" class="list-group-item list-group-item-action py-2">
                <div class="d-flex w-100 align-items-center">
                    <div class="flex-grow-1" style="min-width: 0;">
                        <div class="fw-bold text-truncate">${highlightedId}</div>
                        <small class="text-muted text-truncate d-block">${highlightedName}</small>
                        ${result.description ? `<div class="mt-1 small text-muted text-truncate">${result.description}</div>` : ''}
                        <span class="badge ${badgeClass}">${result.type}</span>
                    </div>
                </div>
            </a>
        `;
    });

    if (results.length > 10) {
        html += `<div class="list-group-item text-center text-muted py-2">
                    ... и еще ${results.length - 10} результатов
                 </div>`;
    }

    searchResults.innerHTML = html;
    searchResults.classList.remove('d-none');

    // Устанавливаем ширину равной ширине input
    searchResults.style.width = searchInput.offsetWidth + 'px';
}

function highlightText(text, query) {
    if (!query || query.length < 2) return text;

    const lowerText = text.toLowerCase();
    const lowerQuery = query.toLowerCase();
    const index = lowerText.indexOf(lowerQuery);

    if (index === -1) return text;

    const before = text.substring(0, index);
    const match = text.substring(index, index + query.length);
    const after = text.substring(index + query.length);

    return `${before}<mark class="bg-warning">${match}</mark>${after}`;
}

// Также подсвечиваем результаты на текущей странице
function highlightOnPage(query) {
    if (!query || query.length < 2) return;

    const elements = document.querySelectorAll('.searchable');
    elements.forEach(el => {
        const text = el.textContent.toLowerCase();
        if (text.includes(query.toLowerCase())) {
            el.classList.add('search-highlight');
        } else {
            el.classList.remove('search-highlight');
        }
    });
}
