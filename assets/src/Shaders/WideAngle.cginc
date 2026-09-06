// Convert a geographic coordinate on the globe surface to a ray in the unit sphere
float3 GeoToCartesian(float lat, float lon) {
    return normalize(float3( 
        cos(lat) * sin(lon),
        sin(lat),
        cos(lat) * cos(lon)
    ));
}
// me when I factor out literally one function
