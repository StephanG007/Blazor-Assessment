let gridInstance = null;

const LOAD_PARAMS = [
    "skip",
    "take",
    "requireTotalCount",
    "requireGroupCount",
    "sort",
    "filter",
    "totalSummary",
    "group",
    "groupSummary"
];

function buildQuery(loadOptions) {
    const params = [];

    LOAD_PARAMS.forEach((name) => {
        const value = loadOptions[name];

        if (value === undefined || value === null) {
            return;
        }

        if (Array.isArray(value) && value.length === 0) {
            return;
        }

        params.push(`${encodeURIComponent(name)}=${encodeURIComponent(JSON.stringify(value))}`);
    });

    return params.join("&");
}

function normaliseBaseUrl(url) {
    return (url || "").replace(/\/$/, "");
}

export function initializeUsersGrid(element, options) {
    if (!element) {
        throw new Error("Unable to find the container for the users grid.");
    }

    disposeUsersGrid();

    const config = options || {};
    const baseUrl = normaliseBaseUrl(config.apiBaseUrl);
    const accessToken = config.accessToken;

    if (!baseUrl || !accessToken) {
        return;
    }

    const dataSource = new DevExpress.data.CustomStore({
        key: "id",
        loadMode: "processed",
        load(loadOptions) {
            const query = buildQuery(loadOptions);
            const requestUrl = `${baseUrl}/api/users${query ? `?${query}` : ""}`;

            return fetch(requestUrl, {
                headers: {
                    Authorization: `Bearer ${accessToken}`
                }
            })
                .then((response) => {
                    if (response.status === 401 || response.status === 403) {
                        throw new Error("You are not authorised to view the users list.");
                    }

                    if (!response.ok) {
                        throw new Error("An error occurred while loading users. Please try again.");
                    }

                    return response.json();
                });
        }
    });

    gridInstance = new DevExpress.ui.dxDataGrid(element, {
        dataSource,
        remoteOperations: true,
        columnAutoWidth: true,
        rowAlternationEnabled: true,
        showBorders: true,
        paging: {
            pageSize: 10
        },
        pager: {
            showPageSizeSelector: true,
            allowedPageSizes: [10, 20, 50],
            showInfo: true
        },
        loadPanel: {
            enabled: true
        },
        columns: [
            {
                dataField: "displayName",
                caption: "Name"
            },
            {
                dataField: "region",
                caption: "Region"
            },
            {
                dataField: "country",
                caption: "Country"
            },
            {
                dataField: "gender",
                caption: "Gender",
                calculateCellValue(rowData) {
                    if (rowData.gender === 1) {
                        return "Female";
                    }

                    if (rowData.gender === 0) {
                        return "Male";
                    }

                    return "Unspecified";
                }
            }
        ]
    });
}

export function disposeUsersGrid() {
    if (gridInstance) {
        gridInstance.dispose();
        gridInstance = null;
    }
}
