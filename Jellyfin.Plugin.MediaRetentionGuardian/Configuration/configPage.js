define([], function () {
    'use strict';

    const pluginId = '5c5422c9-396b-4dec-87fa-a8a20f65c549';
    const logPrefix = '[MediaRetentionGuardian]';

    function log(...args) {
        // eslint-disable-next-line no-console
        console.info(logPrefix, ...args);
    }

    function warn(...args) {
        // eslint-disable-next-line no-console
        console.warn(logPrefix, ...args);
    }

    function error(...args) {
        // eslint-disable-next-line no-console
        console.error(logPrefix, ...args);
    }

    function expectApi() {
        const api = window.ApiClient;
        if (!api) {
            error('ApiClient indisponible');
            if (window.Dashboard) {
                window.Dashboard.alert({ message: "ApiClient indisponible. Rechargez la page." });
            }

            return null;
        }

        return api;
    }

    function formatLastRun(timestamp) {
        if (!timestamp || typeof timestamp !== 'string') {
            return 'N/A';
        }

        const trimmed = timestamp.trim();
        const placeholder = trimmed.toLowerCase();
        if (placeholder.includes('day/month/year')) {
            return 'N/A';
        }

        const parsed = new Date(trimmed);
        if (Number.isNaN(parsed.valueOf())) {
            return trimmed || 'N/A';
        }

        const day = String(parsed.getDate()).padStart(2, '0');
        const month = String(parsed.getMonth() + 1).padStart(2, '0');
        const year = parsed.getFullYear();
        const hours = String(parsed.getHours()).padStart(2, '0');
        const minutes = String(parsed.getMinutes()).padStart(2, '0');
        return `${day}/${month}/${year} - ${hours}:${minutes}`;
    }

    function sanitizeOriginalLog(value) {
        if (!value) {
            return '';
        }

        const cleaned = value
            .split(/<br\s*\/?>|\n/i)
            .map(part => part.trim())
            .filter(part => part.length && !part.toLowerCase().includes('day/month/year') && !part.toLowerCase().includes('hours:minutes'));

        return cleaned.join('<br>');
    }

    function setFormValues(view, config) {
        view.querySelector('#EnableRetention').checked = config.EnableRetention || false;
        view.querySelector('#RetentionPath').value = config.RetentionPath || '';
        view.querySelector('#RetentionDays').value = config.RetentionDays || 30;
        const thresholdToggle = view.querySelector('#EnableDiskThreshold');
        if (thresholdToggle) {
            thresholdToggle.checked = config.EnableDiskThreshold || false;
        }
        const summary = view.querySelector('#LastRunSummary');
        if (summary) {
            if (summary.dataset.originalContent === undefined) {
                const initial = sanitizeOriginalLog(summary.innerHTML.trim());
                summary.dataset.originalContent = initial && initial !== 'Aucune exécution enregistrée.' ? initial : '';
            }

            const deletedRaw = config.LastRunDeletedCount;
            const parsedDeleted = Number(deletedRaw);
            const hasDeleted = !Number.isNaN(parsedDeleted) && Number.isFinite(parsedDeleted);
            const lastRunRaw = typeof config.LastRunUtc === 'string' ? config.LastRunUtc : null;

            if (hasDeleted && parsedDeleted >= 0) {
                const lastRunDisplay = lastRunRaw && lastRunRaw.length ? formatLastRun(lastRunRaw) : 'N/A';
                const thresholdNote = config.LastRunTriggeredByDiskThreshold ? ' (déclenché par le seuil disque)' : '';
                const message = 'Dernière tâche : ' + parsedDeleted + ' fichier(s) supprimé(s) le ' + lastRunDisplay + thresholdNote;
                const original = sanitizeOriginalLog(summary.dataset.originalContent || '');
                summary.dataset.originalContent = original;
                summary.innerHTML = original ? `${message}<br>${original}` : message;
            } else {
                summary.textContent = 'Aucune exécution enregistrée.';
            }
        }
    }

    function parentPath(path) {
        if (!path || path === '/' || /^[A-Za-z]:\\\\?$/.test(path)) {
            return '/';
        }

        let normalized = path.replace(/\\\\/g, '/');
        while (normalized.length > 1 && normalized.endsWith('/')) {
            normalized = normalized.slice(0, -1);
        }

        const lastSlash = normalized.lastIndexOf('/');
        return lastSlash <= 0 ? '/' : normalized.substring(0, lastSlash);
    }

    function normalizeEntries(result) {
        if (!result) {
            return [];
        }

        if (Array.isArray(result.Directories)) {
            return result.Directories;
        }

        if (Array.isArray(result.Items)) {
            return result.Items;
        }

        return Array.isArray(result) ? result : [];
    }

    function createOverlay() {
        const overlay = document.createElement('div');
        overlay.classList.add('mrOverlay');
        overlay.style.setProperty('position', 'fixed');
        overlay.style.setProperty('top', '0');
        overlay.style.setProperty('right', '0');
        overlay.style.setProperty('bottom', '0');
        overlay.style.setProperty('left', '0');
        overlay.style.setProperty('display', 'flex');
        overlay.style.setProperty('justify-content', 'center');
        overlay.style.setProperty('align-items', 'center');
        overlay.style.setProperty('padding', '2rem 0 2rem 18rem');
        overlay.style.setProperty('background', 'rgba(0, 0, 0, 0.6)');
        overlay.style.setProperty('z-index', '100000');
        overlay.style.setProperty('box-sizing', 'border-box');
        return overlay;
    }

    function createDialog() {
        const dialog = document.createElement('div');
        dialog.classList.add('mrDialog');
        dialog.style.setProperty('width', '520px');
        dialog.style.setProperty('max-width', '92vw');
        dialog.style.setProperty('max-height', '80vh');
        dialog.style.setProperty('display', 'flex');
        dialog.style.setProperty('flex-direction', 'column');
        dialog.style.setProperty('gap', '0.75em');
        dialog.style.setProperty('background', 'var(--theme-page-background, #101010)');
        dialog.style.setProperty('color', 'inherit');
        dialog.style.setProperty('border-radius', '10px');
        dialog.style.setProperty('box-shadow', '0 10px 36px rgba(0, 0, 0, 0.45)');
        dialog.style.setProperty('padding', '1.5em');
        return dialog;
    }

    function createHeader() {
        const header = document.createElement('div');
        header.classList.add('mrHeader');
        header.style.setProperty('display', 'flex');
        header.style.setProperty('align-items', 'center');
        header.style.setProperty('gap', '0.5em');
        header.style.setProperty('font-size', '1.1em');
        header.style.setProperty('font-weight', '600');

        const backButton = document.createElement('button');
        backButton.type = 'button';
        backButton.textContent = '←';
        backButton.style.setProperty('background', 'none');
        backButton.style.setProperty('border', 'none');
        backButton.style.setProperty('color', 'inherit');
        backButton.style.setProperty('font-size', '1.25em');
        backButton.style.setProperty('cursor', 'pointer');
        backButton.style.setProperty('padding', '0.2em 0.4em');

        const headerTitle = document.createElement('span');
        headerTitle.textContent = 'Sélectionner un chemin';

        header.appendChild(backButton);
        header.appendChild(headerTitle);

        return { header, backButton };
    }

    function createPathField() {
        const field = document.createElement('div');
        field.classList.add('mrField');
        field.style.setProperty('display', 'flex');
        field.style.setProperty('flex-direction', 'column');
        field.style.setProperty('gap', '0.4em');

        const label = document.createElement('label');
        label.textContent = 'Dossier';

        const input = document.createElement('input');
        input.type = 'text';
        input.setAttribute('is', 'emby-input');
        input.readOnly = true;

        field.appendChild(label);
        field.appendChild(input);
        return { field, input };
    }

    function createList() {
        const list = document.createElement('div');
        list.classList.add('mrList');
        list.style.setProperty('flex', '1');
        list.style.setProperty('overflow-y', 'auto');
        list.style.setProperty('border', '1px solid rgba(255, 255, 255, 0.1)');
        list.style.setProperty('border-radius', '6px');
        list.style.setProperty('padding', '0.5em');
        return list;
    }

    function createFooter() {
        const footer = document.createElement('div');
        footer.classList.add('mrFooter');
        footer.style.setProperty('display', 'flex');
        footer.style.setProperty('justify-content', 'flex-end');
        footer.style.setProperty('gap', '0.5em');

        const cancelButton = document.createElement('button');
        cancelButton.type = 'button';
        cancelButton.classList.add('emby-button');
        cancelButton.textContent = 'Annuler';

        const selectButton = document.createElement('button');
        selectButton.type = 'button';
        selectButton.classList.add('emby-button', 'raised');
        selectButton.textContent = 'OK';

        footer.appendChild(cancelButton);
        footer.appendChild(selectButton);

        return { footer, cancelButton, selectButton };
    }

    function renderEntries(list, entries, loadDirectory) {
        list.innerHTML = '';
        if (!entries.length) {
            const empty = document.createElement('div');
            empty.classList.add('mrEntry');
            empty.style.setProperty('opacity', '0.7');
            empty.textContent = 'Aucun sous-dossier.';
            list.appendChild(empty);
            return;
        }

        entries.forEach(entry => {
            const entryPath = entry.Path || entry.FullName || entry.VirtualPath || entry;
            if (!entryPath) {
                return;
            }

            const button = document.createElement('button');
            button.type = 'button';
            button.classList.add('mrEntry');
            button.textContent = entry.Name || entryPath;
            button.addEventListener('click', () => loadDirectory(entryPath));
            list.appendChild(button);
        });
    }

    function openFallback(api, retentionInput, startPath) {
        const existing = document.querySelector('.mrOverlay');
        if (existing) {
            existing.remove();
        }

        const overlay = createOverlay();
        const dialog = createDialog();
        const { header, backButton } = createHeader();
        const { field: pathField, input: pathDisplay } = createPathField();
        const errorBox = document.createElement('div');
        errorBox.classList.add('mrError');
        errorBox.style.setProperty('color', '#e74c3c');
        errorBox.style.setProperty('min-height', '1em');
        const list = createList();
        const { footer, cancelButton, selectButton } = createFooter();

        dialog.appendChild(header);
        dialog.appendChild(pathField);
        dialog.appendChild(errorBox);
        dialog.appendChild(list);
        dialog.appendChild(footer);
        overlay.appendChild(dialog);
        document.body.appendChild(overlay);

        let currentPath = startPath || '/';

        function cleanup() {
            overlay.remove();
            document.removeEventListener('keydown', keyHandler);
        }

        function keyHandler(evt) {
            if (evt.key === 'Escape') {
                cleanup();
            }
        }

        document.addEventListener('keydown', keyHandler);
        overlay.addEventListener('click', evt => {
            if (evt.target === overlay) {
                cleanup();
            }
        });
        cancelButton.addEventListener('click', cleanup);

        function setCurrentPath(path) {
            currentPath = path;
            pathDisplay.value = path;
        }

        function loadDirectory(path) {
            const effectivePath = path && path.length ? path : '/';
            setCurrentPath(effectivePath);
            errorBox.textContent = '';

            api.getJSON(api.getUrl('Environment/DirectoryContents', {
                path: effectivePath,
                includeDirectories: true,
                includeFiles: false
            })).then(result => {
                const entries = normalizeEntries(result);
                renderEntries(list, entries, loadDirectory);
            }).catch(err => {
                warn('DirectoryContents failed', err);
                if (effectivePath !== '/') {
                    loadDirectory('/');
                } else {
                    errorBox.textContent = "Impossible de charger ce dossier.";
                    list.innerHTML = '';
                }
            });
        }

        backButton.addEventListener('click', () => {
            loadDirectory(parentPath(currentPath));
        });

        selectButton.addEventListener('click', () => {
            retentionInput.value = currentPath;
            cleanup();
        });

        api.getJSON(api.getUrl('Environment/DefaultDirectoryBrowser')).then(result => {
            const defaultPath = result?.Path || result || startPath || '/';
            loadDirectory(defaultPath);
        }).catch(() => {
            loadDirectory(startPath || '/');
        });
    }

    return function init(view) {
        log('Initialisation de la page de configuration');

        const formElement = view.querySelector('#MediaRetentionConfigForm');
        const browseButton = view.querySelector('#BrowseRetentionPath');

        if (!formElement || !browseButton) {
            warn('Formulaire ou bouton de parcours introuvable.');
            return;
        }

        const api = expectApi();
        if (!api) {
            return;
        }

        view.addEventListener('viewshow', () => {
            api.getPluginConfiguration(pluginId).then(config => {
                log('Configuration chargée', config);
                setFormValues(view, config);
            }).catch(err => {
                warn('Échec du chargement de la configuration', err);
            });
        });

        browseButton.addEventListener('click', event => {
            event.preventDefault();
            const retentionInput = view.querySelector('#RetentionPath');
            const initialPath = (retentionInput.value || '').trim() || '/';

            const loader = typeof window.require === 'function'
                ? window.require
                : (typeof window.requirejs === 'function' ? window.requirejs : null);

            if (loader) {
                loader(
                    ['components/directorybrowser/directorybrowser'],
                    function (DirectoryBrowserModule) {
                        const DirectoryBrowser = DirectoryBrowserModule && DirectoryBrowserModule.default
                            ? DirectoryBrowserModule.default
                            : DirectoryBrowserModule;

                        if (!DirectoryBrowser) {
                            warn('Module directorybrowser introuvable, utilisation du fallback.');
                            openFallback(api, retentionInput, initialPath);
                            return;
                        }

                        const picker = new DirectoryBrowser();
                        picker.show({
                            includeDirectories: true,
                            includeFiles: false,
                            header: 'Sélectionner un chemin',
                            path: initialPath,
                            callback: function (path) {
                                if (path) {
                                    retentionInput.value = path;
                                }

                                picker.close();
                            }
                        });
                    },
                    function (err) {
                        warn('Chargement du module directorybrowser échoué, utilisation du fallback.', err);
                        openFallback(api, retentionInput, initialPath);
                    });
                return;
            }

            warn('Module loader indisponible, utilisation du fallback.');
            openFallback(api, retentionInput, initialPath);
        });

        formElement.addEventListener('submit', event => {
            event.preventDefault();

            const enable = view.querySelector('#EnableRetention').checked;
            const pathValue = view.querySelector('#RetentionPath').value.trim();
            const days = parseInt(view.querySelector('#RetentionDays').value, 10) || 30;

            function saveConfiguration() {
                Dashboard.showLoadingMsg();
                api.getPluginConfiguration(pluginId).then(config => {
                    config.EnableRetention = enable;
                    config.RetentionPath = pathValue;
                    config.RetentionDays = days;
                    api.updatePluginConfiguration(pluginId, config).then(result => {
                        Dashboard.processPluginConfigurationUpdateResult(result);
                    });
                }).catch(err => {
                    error('Échec de la mise à jour de la configuration', err);
                    Dashboard.hideLoadingMsg();
                });
            }

            if (!pathValue) {
                saveConfiguration();
                return false;
            }

            Dashboard.showLoadingMsg();
            const validateUrl = api.getUrl('Environment/ValidatePath', {
                path: pathValue,
                validateWriteable: false
            });

            api.ajax({
                type: 'GET',
                url: validateUrl,
                contentType: 'application/json'
            }).then(() => {
                saveConfiguration();
            }).catch(err => {
                warn('ValidatePath failed', err);
                Dashboard.hideLoadingMsg();
                Dashboard.alert({ message: "Le chemin indiqué n'est pas accessible par Jellyfin." });
            });

            return false;
        });
    };
});
