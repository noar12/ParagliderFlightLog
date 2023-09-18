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
	var marker = L.marker([lat, lon], { title: title }).addTo(map);
	return map;
}