let themeState = {
    isDark: false,
    isTestRole: false,
    isAuthenticated: false
};

function applyTheme(isDark) {
    themeState.isDark = isDark;
    document.documentElement.dataset.theme = isDark ? 'dark' : 'light';
    syncThemeToggle(isDark);
}

function syncThemeToggle(isDark) {
    const toggle = document.getElementById('themeToggle');
    if (toggle) {
        toggle.checked = isDark;
    }

    const themeSelect = document.getElementById('themeSelect');
    if (themeSelect) {
        themeSelect.value = isDark ? 'true' : 'false';
    }
}

function initTheme(config) {
    themeState.isTestRole = config.isTestRole === true;
    themeState.isAuthenticated = config.isAuthenticated === true;

    const initialDark = themeState.isTestRole ? false : config.isDarkFromServer === true;
    applyTheme(initialDark);
}

function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : '';
}

async function saveTheme(isDark) {
    applyTheme(isDark);

    if (!themeState.isAuthenticated || themeState.isTestRole) {
        return {
            savedToDatabase: false,
            message: themeState.isTestRole
                ? 'Tema aplicado. Los usuarios de prueba no guardan preferencias en la base de datos.'
                : 'Tema aplicado.'
        };
    }

    const formData = new FormData();
    formData.append('isDarkTheme', isDark);
    formData.append('__RequestVerificationToken', getAntiForgeryToken());

    const response = await fetch('/User/UpdateTheme', {
        method: 'POST',
        body: formData
    });

    const data = await response.json();

    if (!response.ok) {
        throw new Error(data.message || 'No se pudo guardar el tema.');
    }

    return data;
}

async function toggleThemeFromNavbar() {
    const toggle = document.getElementById('themeToggle');
    if (!toggle) return;

    const isDark = toggle.checked;

    try {
        await saveTheme(isDark);
    } catch (error) {
        applyTheme(!isDark);
        if (typeof showErrorAlert === 'function') {
            await showErrorAlert('Error', error.message);
        }
    }
}

document.addEventListener('DOMContentLoaded', function () {
    if (window.__themeInit) {
        initTheme(window.__themeInit);
    }

    const toggle = document.getElementById('themeToggle');
    if (toggle) {
        toggle.addEventListener('change', toggleThemeFromNavbar);
    }
});
