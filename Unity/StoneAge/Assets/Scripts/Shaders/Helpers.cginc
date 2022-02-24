float4 blendColors(float4 source, float4 destination) {
    return source * source.a + destination * (1 - source.a);
}

float4 blendMult(float4 source, float4 dest, float fac) {
    return source * ((1 - fac) + fac * dest);
}

float findMinUntiledDistance(float2 tiledPoint, float2 targetPoint, float bound) {
    bool targetAboveMiddle = targetPoint.y > bound * 0.5;
    bool targetRightOfMiddle = targetPoint.x > bound * 0.5;

    float dist = 0.0f;

    if (targetAboveMiddle && targetRightOfMiddle) {
        // Target is in upper right quadrant.
        // Tiled in upper left quadrant.
        float dist1 = distance(targetPoint, float2(tiledPoint.x + bound, tiledPoint.y));
        // Tiled in lower left quadrant.
        float dist2 = distance(targetPoint, float2(tiledPoint.x + bound, tiledPoint.y + bound));
        // Tiled in lower right quadrant.
        float dist3 = distance(targetPoint, float2(tiledPoint.x, tiledPoint.y + bound));
        dist = min(dist1,  min(dist2, dist3));
    } else if (targetAboveMiddle && !targetRightOfMiddle) {
        // Target is in upper left quadrant.
        // Tiled in upper right quadrant.
        float dist1 = distance(targetPoint, float2(tiledPoint.x - bound, tiledPoint.y));
        // Tiled in lower left quadrant.
        float dist2 = distance(targetPoint, float2(tiledPoint.x, tiledPoint.y + bound));
        // Tiled in lower right quadrant.
        float dist3 = distance(targetPoint, float2(tiledPoint.x - bound, tiledPoint.y + bound));
        dist = min(dist1,  min(dist2, dist3));
    } else if (!targetAboveMiddle && !targetRightOfMiddle) {
        // Target is in lower left quadrant.
        // Tiled in upper right quadrant.
        float dist1 = distance(targetPoint, float2(tiledPoint.x - bound, tiledPoint.y - bound));
        // Tiled in upper left quadrant.
        float dist2 = distance(targetPoint, float2(tiledPoint.x, tiledPoint.y - bound));
        // Tiled in lower right quadrant.
        float dist3 = distance(targetPoint, float2(tiledPoint.x - bound, tiledPoint.y));
        dist = min(dist1,  min(dist2, dist3));
    } else if (!targetAboveMiddle && targetRightOfMiddle) {
        // Target is in lower right quadrant.
        // Tiled in upper right quadrant.
        float dist1 = distance(targetPoint, float2(tiledPoint.x, tiledPoint.y - bound));
        // Tiled in upper left quadrant.
        float dist2 = distance(targetPoint, float2(tiledPoint.x + bound, tiledPoint.y - bound));
        // Tiled in lower left quadrant.
        float dist3 = distance(targetPoint, float2(tiledPoint.x + bound, tiledPoint.y));
        dist = min(dist1,  min(dist2, dist3));
    }

    return dist;
}

float4 getNeighborCoords(float invSize, float2 uv) {
    float leftX = uv.x - 1 * invSize;
    float upY = uv.y + 1 * invSize;
    float rightX = uv.x + 1 * invSize;
    float downY = uv.y - 1 * invSize;

    return float4(leftX, upY, rightX, downY);
}

float4x4 getNeighborsCross(sampler2D tex, float invSize, float2 uv) {
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

float4x4 getNeighborsX(sampler2D tex, float invSize, float2 uv) {
    float4 neighborCoords = getNeighborCoords(invSize, uv);
    
    float4 neighborUL = tex2D(tex, float2(neighborCoords.x, neighborCoords.y));
    float4 neighborUR = tex2D(tex, float2(neighborCoords.z, neighborCoords.y));
    float4 neighborDR = tex2D(tex, float2(neighborCoords.z, neighborCoords.w));
    float4 neighborDL = tex2D(tex, float2(neighborCoords.x, neighborCoords.w));

    float4x4 result;
    result[0] = neighborUL;
    result[1] = neighborUR;
    result[2] = neighborDR;
    result[3] = neighborDL;

    return result;
}

float3 hsv2rgb(float3 c) {
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float3 rgb2hsv(float3 c) {
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float smoothMin(float a, float b, float c) {
    float result = 0.0;

    if (c != 0.0) {
        float h = max(c - abs(a - b), 0.0) / c;
        result = min(a, b) - h * h * h * c * (1.0 / 6.0);
    } else {
        result = min(a, b);
    }

    return result;
}