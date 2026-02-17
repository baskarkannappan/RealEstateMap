window.leafletInterop = {
    map: null,
    dotNetHelper: null,
    markersLayer: null,
    radiusCircle: null,
    moveHandler: null,

    initMap: function (divId, lat, lng, zoom, dotNetHelper) {
        this.disposeMap();

        this.dotNetHelper = dotNetHelper;
        this.map = L.map(divId, { center: [lat, lng], zoom: zoom });

        const lightMap = L.tileLayer(
            'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png',
            { maxZoom: 20, attribution: '&copy; OpenStreetMap &copy; CartoDB' });

        const satellite = L.tileLayer(
            'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}',
            { maxZoom: 20, attribution: 'Tiles © Esri' });

        lightMap.addTo(this.map);
        L.control.layers({ "Light Map": lightMap, "Satellite": satellite }, null, { collapsed: false }).addTo(this.map);

        this.markersLayer = L.markerClusterGroup({
            maxClusterRadius: 50,
            spiderfyOnMaxZoom: true,
            showCoverageOnHover: false,
            zoomToBoundsOnClick: true
        });

        this.map.addLayer(this.markersLayer);

        this.moveHandler = () => {
            if (!this.map || !this.dotNetHelper) {
                return;
            }

            const bounds = this.map.getBounds();
            this.dotNetHelper.invokeMethodAsync(
                "MapMoved",
                bounds.getSouth(),
                bounds.getWest(),
                bounds.getNorth(),
                bounds.getEast(),
                this.map.getZoom())
                .catch(error => console.warn("MapMoved callback failed", error));
        };

        this.map.on("moveend", this.moveHandler);
        this.moveHandler();
    },

    clearLayers: function () {
        if (this.markersLayer) {
            this.markersLayer.clearLayers();
        }
    },

    addMarker: function (lat, lng, text, color) {
        if (!this.markersLayer) {
            return;
        }

        const marker = L.circleMarker([lat, lng], {
            radius: 8,
            color: color || "#1f77b4",
            fillColor: color || "#1f77b4",
            fillOpacity: 0.8,
            weight: 1
        }).bindPopup(text);

        this.markersLayer.addLayer(marker);
    },

    addClusterCount: function (lat, lng, count) {
        if (!this.markersLayer) {
            return;
        }

        const icon = L.divIcon({
            html: `<div class="cluster-pin">${count}</div>`,
            className: "",
            iconSize: [40, 40],
            iconAnchor: [20, 40]
        });

        this.markersLayer.addLayer(L.marker([lat, lng], { icon: icon }));
    },

    showSearchResult: function (lat, lng, zoom, radiusKm) {
        if (!this.map) {
            return;
        }

        this.map.setView([lat, lng], zoom || 12);

        if (this.radiusCircle) {
            this.map.removeLayer(this.radiusCircle);
            this.radiusCircle = null;
        }

        if (radiusKm && radiusKm > 0) {
            this.radiusCircle = L.circle([lat, lng], {
                radius: radiusKm * 1000,
                color: "#ff7f0e",
                fillColor: "#ff7f0e",
                fillOpacity: 0.15,
                weight: 2
            }).addTo(this.map);
        }
    },

    disposeMap: function () {
        if (this.map && this.moveHandler) {
            this.map.off("moveend", this.moveHandler);
        }

        if (this.map) {
            this.map.remove();
        }

        this.map = null;
        this.moveHandler = null;
        this.markersLayer = null;
        this.radiusCircle = null;
        this.dotNetHelper = null;
    }
};
