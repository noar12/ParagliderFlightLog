export function load_map(mapId, lat, lon, zoom) {
    let map = L.map(mapId).setView({lat: lat, lon: lon}, zoom);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    return map;
}

export function modify_map(map, lat, lon, zoom) {
    map.setView({lat: lat, lon: lon}, zoom);
    return map;
}

export function add_marker(map, lat, lon, title) {
    L.marker([lat, lon], {title: title}).addTo(map);
    return map;
}

export function remove_all_polyline(map) {
    map.eachLayer(function (layer) {
        // Check if the layer is an instance of L.Polyline
        if (layer instanceof L.Polyline) {
            // Remove the polyline from the map
            map.removeLayer(layer);
        }
    });
    return map;
}
export function remove_all_marker(map) {
    map.eachLayer(function (layer) {
        // Check if the layer is an instance of L.Polyline
        if (layer instanceof L.Marker) {
            // Remove the marker from the map
            map.removeLayer(layer);
        }
    });
    return map;
}

export function add_polyline(map, latlngs) {
    let polyline = L.polyline(latlngs, {color: 'red'}).addTo(map);
    map.fitBounds(polyline.getBounds());
    return map;
}

export function remove_all(map) {
    map.eachLayer(function (layer) {
        map.removeLayer(layer);
    });
    return map;
}

export function add_geojson(map, geojson) {
    L.geoJSON(geojson, {
        pointToLayer: function (feature, latlng) {
            let label = String(feature.properties.id)
            return new L.marker(latlng, {title: label})
        }
    }).addTo(map);
    return map;
}

const icon = L.icon({
    iconUrl: 'paragliding_24dp_5F6368_FILL0_wght400_GRAD0_opsz24.png',
    iconSize: [21, 34], // Size of the icon 
    iconAnchor: [10, 34], // Anchor point of the icon
    popupAnchor: [1, -34] // Anchor point of the popup relative to the icon
});

export function addOrMoveMarker(map, lat, lng, markerName) {
    let marker = null;
    map.eachLayer(function (layer) {
        if (layer instanceof L.Marker && layer.options.name === markerName) {
            marker = layer;
        }
    });

    if (marker) {
        // Move the existing marker to the new location
        marker.setLatLng([lat, lng]);
    } else {
        // Add a new marker at the given location
        marker = L.marker([lat, lng], {icon: icon, name: markerName}).addTo(map);
    }

    return map;
}