// Этот фрагментный шейдер реализует сложную модель освещения для основного куба:
// Определяет структуры:
// Material - содержит диффузную и зеркальную текстуры, а также коэффициент блеска
// Light - описывает точечный источник света (позиция, ambient/diffuse/specular компоненты)
// SpotLight - описывает прожектор (направление, углы, затухание и т.д.)
// Функции освещения:
// calculatePointLight() - вычисляет освещение от точечного источника
// calculateSpotLight() - вычисляет освещение от прожектора (фонарика)
// Основная логика:
// Комбинирует освещение от точечного источника и прожектора (если он включён)
// Использует текстуры для диффузного и зеркального освещения
// Учитывает положение наблюдателя для расчёта бликов
#version 330 core
out vec4 FragColor;

struct Material {
    sampler2D diffuse;
    sampler2D specular;    
    float shininess;
}; 

struct Light {
    vec3 position;
    vec3 ambient;// фоновое освещение (сцена)
    vec3 diffuse;// рассеянное (свет который попадает на поверхность и рассеивается)
    vec3 specular;// зеркальное (блики на поверхности)
};

struct SpotLight {
    bool on;
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float constant;
    float linear;
    float quadratic;
};

in vec3 FragPos;  
in vec3 Normal;  
in vec2 TexCoords;
  
uniform vec3 viewPos;
uniform Material material;
uniform Light light;
uniform SpotLight spotLight;

vec3 calculatePointLight(Light light, vec3 normal, vec3 fragPos, vec3 viewDir) {
    // Ambient
    vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;
    
    // Diffuse
    vec3 lightDir = normalize(light.position - fragPos);
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * texture(material.diffuse, TexCoords).rgb;
    
    // Specular
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * texture(material.specular, TexCoords).rgb;
    
    return ambient + diffuse + specular;
}

vec3 calculateSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir) {
    if (!light.on) return vec3(0.0);
    
    // Ambient
    vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;
    
    // Diffuse
    vec3 lightDir = normalize(light.position - fragPos);
    float theta = dot(lightDir, normalize(-light.direction));
    float epsilon = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * texture(material.diffuse, TexCoords).rgb;
    
    // Specular
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * texture(material.specular, TexCoords).rgb;
    
    // Attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;
    
    return ambient + diffuse + specular;
}

void main() {
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 result = calculatePointLight(light, Normal, FragPos, viewDir);
    
    if (spotLight.on) {
        result += calculateSpotLight(spotLight, Normal, FragPos, viewDir);
    }
    
    FragColor = vec4(result, 1.0);
}