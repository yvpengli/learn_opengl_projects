#version 330 core
out vec4 FragColor;

struct Material {
    sampler2D diffuse;
    sampler2D specular;    
    float shininess;
}; 

struct DirLight{
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
uniform DirLight dirLight;

struct PointLight {
    vec3 position;

    float constant;
    float linear;
    float quadratic;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
#define NR_POINT_LIGHTS 4
uniform PointLight pointLights[NR_POINT_LIGHTS];

struct SpotLight {
    vec3 direction;
    vec3 position;
    float cutOff;
    float outerCutOff;

    float constant;
    float linear;
    float quadratic;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

};
uniform SpotLight spotLight;

in vec3 FragPos;  
in vec3 Normal;  
in vec2 TexCoords;
  
uniform vec3 viewPos;
uniform Material material;

vec3 calDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 calPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 calSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main()
{
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize( viewPos - FragPos);

    // phase one: directed light
    vec3 result = calDirLight(dirLight, norm, viewDir);

    // phase two: point light
    for (int i = 0; i < NR_POINT_LIGHTS; i ++)
        result += calPointLight(pointLights[i], norm, FragPos, viewDir);

    result += calSpotLight(spotLight, norm, FragPos, viewDir);
    
    FragColor = vec4(result, 1.0);
} 

vec3 calDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
    // lightDir need add -, and normalize
    vec3 lightDir = normalize(-light.direction);

    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));

    // viewDir is fragPoint to viewPoint
    float diff = max( dot(viewDir, lightDir), 0.0 );
    vec3 diffuse = diff * vec3(texture(material.diffuse, TexCoords)) * light.diffuse;

    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow( max(dot(reflectDir, viewDir), 0.0), material.shininess);
    vec3 specular = spec * light.specular * vec3(texture(material.specular, TexCoords ));

    return ambient + diffuse + specular;
}

vec3 calPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);

    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));

    float diff = max( dot(viewDir, lightDir), 0.0 );
    vec3 diffuse = diff * vec3(texture(material.diffuse, TexCoords)) * light.diffuse;

    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow( max(dot(reflectDir, viewDir), 0.0), material.shininess);
    vec3 specular = spec * light.specular * vec3(texture(material.specular, TexCoords ));

    float distance = length( light.position - fragPos );
    float attenuation = 1.0 / ( light.constant + light.linear * distance
                        + light.quadratic * distance * distance);

    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    
    return ambient + diffuse + specular;
}

vec3 calSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);

    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));

    float diff = max( dot(viewDir, lightDir), 0.0 );
    vec3 diffuse = diff * vec3(texture(material.diffuse, TexCoords)) * light.diffuse;

    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow( max(dot(reflectDir, viewDir), 0.0), material.shininess);
    vec3 specular = spec * light.specular * vec3(texture(material.specular, TexCoords ));

    float distance = length( light.position - fragPos );
    float attenuation = 1.0 / ( light.constant + light.linear * distance
                        + light.quadratic * distance * distance);

    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    
    float theta = dot(lightDir, normalize(-light.direction));
    float epsilon = light.cutOff - light.outerCutOff;
    float intensity = clamp( (theta - light.outerCutOff)/epsilon, 0.0, 1.0 );
    
    ambient *= intensity;
    diffuse *= intensity;
    specular *= intensity;

    return ambient + diffuse + specular;
}
