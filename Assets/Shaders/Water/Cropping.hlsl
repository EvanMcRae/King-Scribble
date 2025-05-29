//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

float ObjCropping_float(float3 Pos, Texture2D Objs, float WorldY, float StartX, float Interval, float Num, out float Out)
{
    // Initial variables/data
    float prevTop, top = 0, nextTop, left, right, center;
    float4 info = Objs.Load(int3(0, 0, 0)), nextInfo;

    // Queue top data for first iteration
    nextTop = info[0] * 255 + info[1];
    if (info[2] < 1) nextTop *= -1;
    nextTop += WorldY;

    // Loop over all positions
    for (int i = 0; i < Num; i++)
    {
        // Load pixel data for next position
        nextInfo = Objs.Load(int3(i+1, 0, 0));

        // Decode next top and relay values to previous tops
        prevTop = top;
        top = nextTop;
        nextTop = nextInfo[0] * 255 + nextInfo[1];
        if (nextInfo[2] < 1) nextTop *= -1;
        nextTop += WorldY;

        // Decode center and extent to calculate left and right bounds
        center = StartX + i * Interval;
        left = center - Interval;
        right = center + Interval;

        // If between the left and right bounds, check for crop
        if (Pos[0] > left && Pos[0] < right)
        {
            // Smooth top bounds by lerping over distance from either side of center
            float alpha = 0;
            if (Pos[0] < center)
            {
                alpha = (Pos[0] - left) / (center - left);
                top = lerp(prevTop, top, alpha);
            }
            else
            {
                alpha = (right - Pos[0]) / (right - center);
                top = lerp(nextTop, top, alpha);
            }

            // If under the top bound, crop 
            if (Pos[1] < top)
            {
                Out = 0;
                return Out;
            }
        }

        // Inherit data for next iteration
        info = nextInfo;
    }

    // Success case
    Out = 1;
    return Out;
}

float ObjCroppingOld_float(float3 Pos, float Top, float Left, float Right, out float Out)
{
    if ((Pos[1] < Top) && (Pos[0] > Left) && (Pos[0] < Right))
    {
        Out = 0;
        return Out;
    }
    Out = 1;
    return Out;
}

#endif //MYHLSLINCLUDE_INCLUDED