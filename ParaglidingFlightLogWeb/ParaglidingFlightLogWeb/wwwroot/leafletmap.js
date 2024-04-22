export function load_map(mapId, lat, lon, zoom) {
	let map = L.map(mapId).setView({ lat: lat, lon: lon }, zoom);
	L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
		maxZoom: 19,
		attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
	}).addTo(map);

	return map;
}
export function modify_map(map, lat, lon, zoom) {
	map.setView({ lat: lat, lon: lon }, zoom);
	return map;
}
export function add_marker(map, lat, lon, title) {
	L.marker([lat, lon], { title: title }).addTo(map);
	return map;
}
export function add_polyline(map, latlngs) {
	let polyline = L.polyline(latlngs, { color: 'red' }).addTo(map);
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
			return new L.marker(latlng, { title: label })
		}
	}).addTo(map);
	return map;
}
