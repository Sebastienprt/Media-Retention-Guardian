
            (function () {
                const pluginId = '5c5422c9-396b-4dec-87fa-a8a20f65c549';
                const logPrefix = '[MediaRetentionGuardian]';

                function log() {
                    // eslint-disable-next-line no-console
                    console.info.apply(console, [logPrefix].concat(Array.from(arguments)));
                }

                function warn() {
                    // eslint-disable-next-line no-console
                    console.warn.apply(console, [logPrefix].concat(Array.from(arguments)));
                }

                function error() {
                    // eslint-disable-next-line no-console
                    console.error.apply(console, [logPrefix].concat(Array.from(arguments)));
                }

                function expectApi() {
                    if (!window.ApiClient) {
                        error('ApiClient indisponible');
                        if (window.Dashboard) {
                            window.Dashboard.alert({ message: "ApiClient indisponible. Rechargez la page." });
                        }
                        return null;
                    }

                    return window.ApiClient;
                }

                function setFormValues(config) {
                    document.querySelector('#EnableRetention').checked = config.EnableRetention || false;
                    document.querySelector('#RetentionPath').value = config.RetentionPath || '';
                    document.querySelector('#RetentionDays').value = config.RetentionDays || 30;
                    const summary = document.querySelector('#LastRunSummary');
                    if (config.LastRunDeletedCount >= 0) {
                        summary.textContent = `Dernière tâche : ${config.LastRunDeletedCount} fichier(s) supprimé(s) le ${config.LastRunUtc ?? 'N/A'}`;
                    }
                }

                function inferNameFromPath(path) {
                    if (!path) {
                        return '';
                    }

                    const normalized = path.replace(/\\/g, '/').replace(/\/+$/, '');
                    const parts = normalized.split('/').filter(Boolean);
                    return parts.length ? parts[parts.length - 1] : normalized;
                }

                function extractPath(value) {
                    if (!value) {
                        return '';
                    }

                    if (typeof value === 'string') {
                        return value;
                    }

                    if (value.Path) {
                        return value.Path;
                    }

                    try {
                        const parsed = JSON.parse(value);
                        return parsed.Path || '';
                    } catch (err) {
                        warn('extractPath JSON parse failed', err);
                        return '';
                    }
                }

                function normalizeDirectoryEntries(result) {
                    if (!result) {
                        return [];
                    }

                    if (Array.isArray(result.Directories)) {
                        return result.Directories;
                    }

                    if (Array.isArray(result.Items)) {
                        return result.Items;
                    }

                    if (Array.isArray(result)) {
                        return result;
                    }

                    return [];
                }

                function renderDirectoryEntries(list, entries) {
                    list.innerHTML = '';
                    if (!entries.length) {
                        const empty = document.createElement('div');
                        empty.classList.add('pathPickerEntry');
                        empty.textContent = 'Aucun sous-dossier.';
                        empty.style.opacity = '0.7';
                        empty.disabled = true;
                        list.appendChild(empty);
                        return;
                    }

                    entries.forEach(entry => {
                        const type = (entry.Type || '').toLowerCase();
                        const isDir = entry.IsDirectory === true || (!entry.IsDirectory && (!type || type.includes('directory')));
                        if (!isDir) {
                            return;
                        }

                        const entryPath = entry.Path || entry.FullName || entry.VirtualPath;
                        if (!entryPath) {
                            return;
                        }

                        const button = document.createElement('button');
                        button.type = 'button';
                        button.classList.add('pathPickerEntry');
                        button.textContent = entry.Name || inferNameFromPath(entryPath) || entryPath;
                        button.addEventListener('click', () => {
                            loadDirectory(entryPath);
                        });
                        list.appendChild(button);
                    });

                    if (!list.hasChildNodes()) {
                        const info = document.createElement('div');
                        info.classList.add('pathPickerEntry');
                        info.textContent = 'Aucun sous-dossier.';
                        info.style.opacity = '0.7';
                        info.disabled = true;
                        list.appendChild(info);
                    }
                }

                function openDirectoryPicker() {
                    const api = expectApi();
                    if (!api) {
                        return;
                    }

                    const existing = document.querySelector('.pathPickerOverlay');
                    if (existing) {
                        existing.remove();
                    }

                    const overlay = document.createElement('div');
                    overlay.classList.add('pathPickerOverlay');

                    const dialog = document.createElement('div');
                    dialog.classList.add('pathPickerDialog');

                    const header = document.createElement('div');
                    header.classList.add('pathPickerHeader');

                    const backButton = document.createElement('button');
                    backButton.type = 'button';
                    backButton.innerHTML = '←';

                    const headerTitle = document.createElement('span');
                    headerTitle.textContent = 'Sélectionner un chemin';

                    header.appendChild(backButton);
                    header.appendChild(headerTitle);

                    const info = document.createElement('div');
                    info.classList.add('pathPickerInfo');
                    info.textContent = '(Chargement...)';

                    const pathField = document.createElement('div');
                    pathField.classList.add('pathPickerPathField');

                    const pathLabel = document.createElement('label');
                    pathLabel.textContent = 'Dossier';

                    const pathInput = document.createElement('input');
                    pathInput.type = 'text';
                    pathInput.setAttribute('is', 'emby-input');
                    pathInput.readOnly = true;
                    pathInput.placeholder = '/';

                    pathField.appendChild(pathLabel);
                    pathField.appendChild(pathInput);

                    const errorBox = document.createElement('div');
                    errorBox.classList.add('pathPickerError');

                    const list = document.createElement('div');
                    list.classList.add('pathPickerList');

                    const footer = document.createElement('div');
                    footer.classList.add('pathPickerFooter');

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

                    dialog.appendChild(header);
                    dialog.appendChild(info);
                    dialog.appendChild(pathField);
                    dialog.appendChild(errorBox);
                    dialog.appendChild(list);
                    dialog.appendChild(footer);

                    overlay.appendChild(dialog);
                    document.body.appendChild(overlay);

                    let currentPath = document.querySelector('#RetentionPath').value.trim() || '/';

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

                    cancelButton.addEventListener('click', cleanup);
                    overlay.addEventListener('click', evt => {
                        if (evt.target === overlay) {
                            cleanup();
                        }
                    });

                    function renderEntries(entries) {
                        list.innerHTML = '';
                        if (!entries.length) {
                            const empty = document.createElement('div');
                            empty.classList.add('pathPickerEntry');
                            empty.style.opacity = '0.7';
                            empty.textContent = 'Aucun sous-dossier.';
                            list.appendChild(empty);
                            return;
                        }

                        entries.forEach(entry => {
                            const entryPath = entry.Path || entry.FullName || entry.VirtualPath;
                            if (!entryPath) {
                                return;
                            }

                            const item = document.createElement('button');
                            item.type = 'button';
                            item.classList.add('pathPickerEntry');
                            const label = entry.Name || inferNameFromPath(entryPath) || entryPath;
                            item.textContent = label;
                            item.addEventListener('click', () => {
                                loadDirectory(entryPath);
                            });
                            list.appendChild(item);
                        });
                    }

                    function loadDirectory(path) {
                        const effectivePath = path || '/';
                        currentPath = effectivePath;
                        info.textContent = effectivePath;
                        pathInput.value = effectivePath;
                        errorBox.textContent = '';

                        if (!effectivePath || effectivePath === '/') {
                            loadRoot();
                            return;
                        }

                        const query = {
                            path: effectivePath,
                            includeFiles: false,
                            includeDirectories: true
                        };

                        api.getJSON(api.getUrl('Environment/DirectoryContents', query)).then(result => {
                            const entries = normalizeDirectoryEntries(result);
                            renderEntries(entries);
                        }).catch(err => {
                            warn('DirectoryContents failed', err);
                            errorBox.textContent = "Impossible de charger ce dossier.";
                            list.innerHTML = '';
                        });
                    }

                    function loadRoot() {
                        currentPath = '/';
                        info.textContent = '/';
                        pathInput.value = '/';

                        api.getJSON(api.getUrl('Environment/GetDrives')).then(drives => {
                            const entries = (drives || []).map(drivePath => ({
                                Path: drivePath,
                                Name: drivePath,
                                IsDirectory: true
                            }));
                            renderEntries(entries);
                            info.textContent = '/';
                            pathInput.value = '/';
                        }).catch(err => {
                            warn('GetDrives failed', err);
                            errorBox.textContent = 'Impossible de récupérer les lecteurs.';
                        });
                    }

                    backButton.addEventListener('click', () => {
                        if (!currentPath || currentPath === '/') {
                            loadRoot();
                            return;
                        }

                        const parts = currentPath.replace(/\/g, '/').split('/').filter(Boolean);
                        parts.pop();
                        const parent = '/' + parts.join('/');
                        loadDirectory(parent === '//' ? '/' : parent);
                    });

                    selectButton.addEventListener('click', () => {
                        const chosen = currentPath || '/';
                        document.querySelector('#RetentionPath').value = chosen;
                        cleanup();
                    });

                    if (!currentPath || currentPath === '/') {
                        loadRoot();
                    } else {
                        loadDirectory(currentPath);
                    }
                }
                document.querySelector('#MediaRetentionConfigPage').addEventListener('pageshow', function () {
                    const api = expectApi();
                    if (!api) {
                        return;
                    }

                    Dashboard.showLoadingMsg();
                    api.getPluginConfiguration(pluginId).then(function (config) {
                        log('Configuration chargée', config);
                        setFormValues(config);
                        Dashboard.hideLoadingMsg();
                    }).catch(err => {
                        warn('Échec du chargement de la configuration', err);
                        Dashboard.hideLoadingMsg();
                    });
                });

                document.querySelector('#BrowseRetentionPath').addEventListener('click', function (evt) {
                    evt.preventDefault();
                    openDirectoryPicker();
                });

                document.querySelector('#MediaRetentionConfigForm').addEventListener('submit', function (e) {
                    e.preventDefault();

                    const api = expectApi();
                    if (!api) {
                        return false;
                    }

                    const enable = document.querySelector('#EnableRetention').checked;
                    const path = document.querySelector('#RetentionPath').value.trim();
                    const days = parseInt(document.querySelector('#RetentionDays').value, 10) || 30;

                    function saveConfiguration() {
                        Dashboard.showLoadingMsg();
                        api.getPluginConfiguration(pluginId).then(function (config) {
                            config.EnableRetention = enable;
                            config.RetentionPath = path;
                            config.RetentionDays = days;
                            api.updatePluginConfiguration(pluginId, config).then(function (result) {
                                Dashboard.processPluginConfigurationUpdateResult(result);
                            });
                        }).catch(err => {
                            error('Échec de la mise à jour de la configuration', err);
                            Dashboard.hideLoadingMsg();
                        });
                    }

                    if (!path) {
                        saveConfiguration();
                        return false;
                    }

                    Dashboard.showLoadingMsg();
                    api.ajax({
                        type: 'POST',
                        url: api.getUrl('Environment/ValidatePath'),
                        data: JSON.stringify({ Path: path, ValidateWriteable: false }),
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
            }());
        