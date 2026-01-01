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

// Обработка ввода в поиске
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

            // Обновляем результаты поиска
            updateSearchResults(results, query);
        }, 300); // Задержка 300ms
    });

    // Закрытие результатов при клике вне
    document.addEventListener('click', function (e) {
        if (!searchResults.contains(e.target) && e.target !== searchInput) {
            searchResults.classList.add('d-none');
        }
    });
});

function updateSearchResults(results, query) {
    const searchResults = document.getElementById('searchResults');
    if (!searchResults) return;

    if (results.length === 0 || query.length < 2) {
        searchResults.classList.add('d-none');
        return;
    }

    // Сортируем результаты
    results.sort((a, b) => {
        // Приоритет: полное совпадение с id
        const aExactId = a.id.toLowerCase() === query.toLowerCase();
        const bExactId = b.id.toLowerCase() === query.toLowerCase();
        if (aExactId && !bExactId) return -1;
        if (!aExactId && bExactId) return 1;

        // Затем по началу строки
        const aStartsWith = a.id.toLowerCase().startsWith(query.toLowerCase());
        const bStartsWith = b.id.toLowerCase().startsWith(query.toLowerCase());
        if (aStartsWith && !bStartsWith) return -1;
        if (!aStartsWith && bStartsWith) return 1;

        // Затем по алфавиту
        return a.id.localeCompare(b.id);
    });

    // Ограничиваем количество результатов
    const displayResults = results.slice(0, 10);

    // Генерируем HTML
    let html = '';

    displayResults.forEach(result => {
        const typeIcon = getTypeIcon(result.type);
        const highlightedName = highlightText(result.name, query);
        const highlightedId = highlightText(result.id, query);

        html += `
            <a href="${result.url}" class="list-group-item list-group-item-action">
                <div class="d-flex w-100 align-items-center">
                    <span class="me-2">${typeIcon}</span>
                    <div class="flex-grow-1">
                        <div class="fw-bold">${highlightedName}</div>
                        <small class="text-muted">${highlightedId}</small>
                        ${result.description ? `<div class="mt-1 small text-truncate">${result.description}</div>` : ''}
                    </div>
                    <span class="badge bg-secondary">${result.type}</span>
                </div>
            </a>
        `;
    });

    if (results.length > 10) {
        html += `<div class="list-group-item text-center text-muted">
                    ... и еще ${results.length - 10} результатов
                 </div>`;
    }

    searchResults.innerHTML = html;
    searchResults.classList.remove('d-none');
}

function getTypeIcon(type) {
    const icons = {
        'class': '📦',
        'property': '🔗'
    };
    return icons[type] || '📄';
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