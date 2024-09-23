
static const float a = 2.51;  // Default: 2.51f
static const float b = 0.03;  // Default: 0.03f
static const float c = 2.43;  // Default: 2.43f
static const float d = 0.59;  // Default: 0.59f
static const float e = 0.14;  // Default: 0.14f
static const float p = 1.3;
static const float overlap = 0.2;

static const float rgOverlap = 0.1 * overlap;
static const float rbOverlap = 0.01 * overlap;
static const float gbOverlap = 0.04 * overlap;

static const float3x3 coneOverlap = float3x3(
                                                1.0,        rgOverlap,  rbOverlap,
                                                rgOverlap,  1.0, 		gbOverlap,
                                                rbOverlap, 	rgOverlap, 	1.0
                                            );

static const float3x3 coneOverlapInverse = float3x3(
                                                        1.0 + (rgOverlap + rbOverlap),  -rgOverlap, 	                -rbOverlap,
                                                        -rgOverlap, 		            1.0 + (rgOverlap + gbOverlap),  -gbOverlap,
                                                        -rbOverlap, 		            -rgOverlap, 	                1.0 + (rbOverlap + rgOverlap)
                                                    );

float3 ACESTonemap(float3 color){
    color = mul(coneOverlap,color);
    color = pow(color, float3(p,p,p));
    color = (color * (a * color + b)) / (color * (c * color + d) + e);
    color = pow(color, float3(1.0,1.0,1.0)/p);
    color = mul(coneOverlapInverse,color);
    color = clamp(color,float3(0.0,0.0,0.0),float3(1.0,1.0,1.0));
    return color;
}