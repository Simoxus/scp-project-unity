float hash13(float3 x)
{
	return frac(sin(dot(x, float3(33.5382, 51.3478, 42.432))) * 321.523);
}

float2 hash22(float2 x)
{
	return frac(sin(mul(x, float2x2(3.5382, 5.3478, 4.432, 4.321))) * 32.523);
}

float noise13(float3 x)
{
	float3 root = floor(x);
			
	float3 f = smoothstep(0.0, 1.0, frac(x));

	float n000 = hash13(root + float3(0,0,0));
	float n001 = hash13(root + float3(0,0,1));
	float n010 = hash13(root + float3(0,1,0));
	float n011 = hash13(root + float3(0,1,1));
	float n100 = hash13(root + float3(1,0,0));
	float n101 = hash13(root + float3(1,0,1));
	float n110 = hash13(root + float3(1,1,0));
	float n111 = hash13(root + float3(1,1,1));

	float n00 = lerp(n000, n001, f.z);
	float n01 = lerp(n010, n011, f.z);
	float n10 = lerp(n100, n101, f.z);
	float n11 = lerp(n110, n111, f.z);

	float n0 = lerp(n00, n01, f.y);
	float n1 = lerp(n10, n11, f.y);

	float n = lerp(n0, n1, f.x);

	return n;
}

float layeredNoise13(float3 x, float iterations, float alphaMod, float sizeMod)
{
	float noise = 0.0;
	float maximum = 0.0;
	for (float i = 0.0; i <= iterations; i += 1.0)
	{
		noise += noise13(x * pow(sizeMod, i)) * pow(alphaMod, i);
		maximum += pow(alphaMod, i);
	}

	return noise / maximum;
}