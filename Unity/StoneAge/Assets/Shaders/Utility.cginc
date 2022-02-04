float4x4 getNeighbors(sampler2D tex, float invSize, float2 uv) {
    float leftX = saturate(uv.x - 1 * invSize);
    float upY = saturate(uv.y + 1 * invSize);
    float rightX = saturate(uv.x + 1 * invSize);
    float downY = saturate(uv.y - 1 * invSize);

    float4 neighborL = tex2D(tex, float2(leftX, uv.y));
    float4 neighborT = tex2D(tex, float2(uv.x, upY));
    float4 neighborR = tex2D(tex, float2(rightX, uv.y));
    float4 neighborB = tex2D(tex, float2(uv.x, downY));

    float4x4 result;
    result[0] = neighborL;
    result[1] = neighborT;
    result[2] = neighborR;
    result[3] = neighborB;

    return result;
}