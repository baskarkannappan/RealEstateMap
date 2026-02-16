window.leafletInterop = {

    map: null,
    dotNetHelper: null,
    markersLayer: null,
    radiusCircle: null,
    baseLayers: {},

    // -----------------------------------------------------
    // Initialize Map
    // -----------------------------------------------------
    initMap: function (divId, lat, lng, zoom, dotNetHelper) {

        this.dotNetHelper = dotNetHelper;

        this.map = L.map(divId, {
            center: [lat, lng],
            zoom: zoom
        });

        // ---------- Base Layers ----------
        const lightMap = L.tileLayer(
            'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png',
            {
                maxZoom: 20,
                attribution: '&copy; OpenStreetMap &copy; CartoDB'
            });

        const satellite = L.tileLayer(
            'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}',
            {
                maxZoom: 20,
                attribution: 'Tiles © Esri'
            });

        // Default map
        lightMap.addTo(this.map);

        this.baseLayers = {
            "Light Map": lightMap,
            "Satellite": satellite
        };

        L.control.layers(this.baseLayers, null, { collapsed: false }).addTo(this.map);

        // ---------- Marker Cluster Layer ----------
        this.markersLayer = L.markerClusterGroup({
            maxClusterRadius: 50, // smaller clusters for closer grouping
            spiderfyOnMaxZoom: true,
            showCoverageOnHover: false,
            zoomToBoundsOnClick: true
        });
        this.map.addLayer(this.markersLayer);

        // ---------- Map Move Event ----------
        this.map.on('moveend', () => {
            if (!this.map || !this.dotNetHelper)
                return;

            const bounds = this.map.getBounds();

            this.dotNetHelper.invokeMethodAsync(
                "MapMoved",
                bounds.getSouth(),
                bounds.getWest(),
                bounds.getNorth(),
                bounds.getEast(),
                this.map.getZoom()
            );
        });
    },

    // -----------------------------------------------------
    // Clear Markers (keep radius intact)
    // -----------------------------------------------------
    clearLayers: function () {
        if (this.markersLayer)
            this.markersLayer.clearLayers();
    },

    // -----------------------------------------------------
    // Add Single Marker
    // -----------------------------------------------------
    // -----------------------------------------------------
    // Add Single Marker (House Icon)
    // -----------------------------------------------------
    addMarker: function (lat, lng, text, color = "#0078ff") {
        if (!this.markersLayer)
            return;

        // Define a custom house icon
        const houseIcon = L.icon({
            iconUrl: 'images/house-icon.png', // <-- add your house icon in wwwroot/images/
            iconSize: [30, 30],
            iconAnchor: [15, 30], // bottom center
            popupAnchor: [0, -30]
        });

        const marker = L.marker([lat, lng], { icon: houseIcon }).bindPopup(text);
        this.markersLayer.addLayer(marker);
    },

    // -----------------------------------------------------
    // Add Cluster Count Pin (Optional Custom Cluster Icon)
    // -----------------------------------------------------
    addClusterCount: function (lat, lng, count) {
        if (!this.markersLayer)
            return;

        const icon = L.divIcon({
            html: `<div class="cluster-pin">${count}</div>`,
            className: "",
            iconSize: [40, 40],
            iconAnchor: [20, 40]
        });

        const marker = L.marker([lat, lng], { icon });
        this.markersLayer.addLayer(marker);
    },

    // -----------------------------------------------------
    // Set Map View
    // -----------------------------------------------------
    setView: function (lat, lng, zoom) {
        if (!this.map)
            return;

        this.map.setView([lat, lng], zoom);
    },

    // -----------------------------------------------------
    // Draw / Update Radius Circle (KM)
    // -----------------------------------------------------
    drawRadius: function (lat, lng, radiusKm) {
        if (!this.map)
            return;

        // Remove previous circle if exists
        if (this.radiusCircle) {
            this.map.removeLayer(this.radiusCircle);
            this.radiusCircle = null;
        }

        // Add new circle
        this.radiusCircle = L.circle([lat, lng], {
            radius: radiusKm * 1000,
            color: "#ff7f0e",    // Orange for search
            fillColor: "#ff7f0e",
            fillOpacity: 0.15,
            weight: 2
        }).addTo(this.map);
    },

    // -----------------------------------------------------
    // Show Search Result (Move + Radius)
    // -----------------------------------------------------
    showSearchResult: function (lat, lng, zoom, radiusKm) {
        if (!this.map)
            return;

        this.map.setView([lat, lng], zoom || 12);

        if (radiusKm && radiusKm > 0) {
            this.drawRadius(lat, lng, radiusKm);
        }
    }
};
