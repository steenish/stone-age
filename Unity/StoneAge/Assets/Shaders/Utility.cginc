float4 getNeighborCoords(float invSize, float2 uv) {
    float leftX = uv.x - 1 * invSize;
    float upY = uv.y + 1 * invSize;
    float rightX = uv.x + 1 * invSize;
    float downY = uv.y - 1 * invSize;

    return float4(leftX, upY, rightX, downY);
}

float4x4 getNeighbors(sampler2D tex, float invSize, float2 uv) {
    float4 neighborCoords = getNeighborCoords(invSize, uv);

    float4 neighborL = tex2D(tex, float2(neighborCoords.x, uv.y));
    float4 neighborT = tex2D(tex, float2(uv.x, neighborCoords.y));
    float4 neighborR = tex2D(tex, float2(neighborCoords.z, uv.y));
    float4 neighborB = tex2D(tex, float2(uv.x, neighborCoords.w));

    float4x4 result;
    result[0] = neighborL;
    result[1] = neighborT;
    result[2] = neighborR;
    result[3] = neighborB;

    return result;
}