#version 330 core
struct Material {
    sampler2D diffuse;
    vec3 specular;
    float shininess;
};
struct PointLight {
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
struct SpotLight {
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;
    float constant;
    float linear;
    float quadratic;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

in vec3 vFragPos;
in vec3 vNormal;
in vec2 vTex;

out vec4 FragColor;

uniform vec3 uViewPos;
uniform Material uMaterial;

uniform PointLight uPoint;
uniform bool uPointOn;

uniform SpotLight uSpot;
uniform bool uSpotOn;

vec3 CalcPhongPoint(PointLight light, vec3 normal, vec3 viewDir, vec3 tex) {
    vec3 lightDir = normalize(light.position - vFragPos);
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), uMaterial.shininess);
    vec3 ambient  = light.ambient * tex;
    vec3 diffuse  = light.diffuse * diff * tex;
    vec3 specular = light.specular * spec * uMaterial.specular;
    return ambient + diffuse + specular;
}

vec3 CalcPhongSpot(SpotLight light, vec3 normal, vec3 viewDir, vec3 tex) {
    vec3 lightDir = normalize(light.position - vFragPos);
    float theta = dot(lightDir, normalize(-light.direction));
    float epsilon = (light.cutOff - light.outerCutOff);
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    float distance = length(light.position - vFragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), uMaterial.shininess);
    vec3 ambient  = light.ambient * tex;
    vec3 diffuse  = light.diffuse * diff * tex;
    vec3 specular = light.specular * spec * uMaterial.specular;
    return (ambient + diffuse + specular) * intensity * attenuation;
}

void main() {
    vec3 tex = texture(uMaterial.diffuse, vTex).rgb;
    vec3 norm = normalize(vNormal);
    vec3 viewDir = normalize(uViewPos - vFragPos);

    vec3 color = vec3(0.0);
    if (uPointOn) color += CalcPhongPoint(uPoint, norm, viewDir, tex);
    if (uSpotOn)  color += CalcPhongSpot(uSpot, norm, viewDir, tex);
    if (!uPointOn && !uSpotOn) color = tex * 0.12;

    FragColor = vec4(color, 1.0);
}
