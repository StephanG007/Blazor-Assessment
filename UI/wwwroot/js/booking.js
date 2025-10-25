let state = null;

function ensureState() {
    if (!state) {
        throw new Error("DevExtreme booking module is not initialized.");
    }
}

function toDate(value) {
    if (!value) {
        return null;
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
}

export function initialize(dotnetRef) {
    state = {
        dotnetRef,
        isSubmitting: false
    };

    const clinicElement = document.getElementById("clinicSelect");
    if (clinicElement) {
        state.clinicSelect = new DevExpress.ui.dxSelectBox(clinicElement, {
            items: [],
            valueExpr: "id",
            displayExpr: (item) => {
                if (!item) {
                    return "";
                }

                const location = item.city && item.province
                    ? ` (${item.city}, ${item.province})`
                    : "";
                return `${item.name}${location}`;
            },
            placeholder: "Select a clinic",
            searchEnabled: true,
            onValueChanged: (e) => {
                if (!e.event) {
                    return;
                }

                const value = typeof e.value === "number" ? e.value : 0;
                state.dotnetRef.invokeMethodAsync("OnClinicChangedFromJs", value);
            }
        });
    }

    const dateElement = document.getElementById("bookingDate");
    if (dateElement) {
        state.datePicker = new DevExpress.ui.dxDateBox(dateElement, {
            type: "date",
            applyValueMode: "instantly",
            displayFormat: "dddd, MMM d",
            min: new Date(),
            onValueChanged: (e) => {
                if (!e.event || !e.value) {
                    return;
                }

                state.dotnetRef.invokeMethodAsync("OnDateChangedFromJs", e.value.toISOString());
            }
        });
    }

    const slotsElement = document.getElementById("slotList");
    if (slotsElement) {
        state.slotList = new DevExpress.ui.dxList(slotsElement, {
            items: [],
            keyExpr: "id",
            displayExpr: "label",
            selectionMode: "single",
            focusStateEnabled: false,
            activeStateEnabled: true,
            onSelectionChanged: (e) => {
                if (!e.event) {
                    return;
                }

                const item = e.addedItems[0];
                const value = item ? item.id : null;
                state.dotnetRef.invokeMethodAsync("OnSlotSelectedFromJs", value);
            }
        });
    }

    const clinicLoader = document.getElementById("clinicLoader");
    if (clinicLoader) {
        state.clinicLoader = new DevExpress.ui.dxLoadIndicator(clinicLoader, {
            height: 42,
            width: 42
        });
    }

    const availabilityLoader = document.getElementById("availabilityLoader");
    if (availabilityLoader) {
        state.availabilityLoader = new DevExpress.ui.dxLoadIndicator(availabilityLoader, {
            height: 30,
            width: 30
        });
    }
}

export function setClinics(clinics, selectedId) {
    ensureState();
    if (!state.clinicSelect) {
        return;
    }

    const items = Array.isArray(clinics) ? clinics : [];
    state.clinicSelect.option("items", items);

    const numericId = typeof selectedId === "string" ? parseInt(selectedId, 10) : selectedId;
    const value = typeof numericId === "number" && !Number.isNaN(numericId) && numericId > 0 ? numericId : null;
    state.clinicSelect.option("value", value);
}

export function setDate(isoDate) {
    ensureState();
    if (!state.datePicker) {
        return;
    }

    const value = toDate(isoDate) ?? new Date();
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    state.datePicker.option({
        value,
        min: today
    });
}

export function setSlots(slots, selectedId, isLoading) {
    ensureState();
    if (!state.slotList) {
        return;
    }

    const items = Array.isArray(slots) ? slots : [];
    state.slotList.option("items", items);

    let key = null;
    if (selectedId != null) {
        const numericId = typeof selectedId === "string" ? parseInt(selectedId, 10) : selectedId;
        key = typeof numericId === "number" && !Number.isNaN(numericId) ? numericId : null;
    }

    state.slotList.option("selectedItemKeys", key != null ? [key] : []);

    const disableList = !!isLoading || state.isSubmitting;
    state.slotList.option("disabled", disableList);

    const slotElement = state.slotList.element();
    if (slotElement) {
        slotElement.classList.toggle("is-loading", !!isLoading);
    }
}

export function setSubmissionState(isSubmitting) {
    ensureState();
    state.isSubmitting = !!isSubmitting;

    if (state.clinicSelect) {
        state.clinicSelect.option("disabled", state.isSubmitting);
    }

    if (state.datePicker) {
        state.datePicker.option("disabled", state.isSubmitting);
    }

    if (state.slotList) {
        const slotElement = state.slotList.element();
        const currentlyLoading = slotElement ? slotElement.classList.contains("is-loading") : false;
        state.slotList.option("disabled", state.isSubmitting || currentlyLoading);
    }
}
