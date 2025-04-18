#version 330 core

in vec3 FragPos;
in vec3 Normal;

uniform vec3 color;
uniform vec3 lightDir;
uniform vec3 viewPos;

out vec4 FragColor;

void main()
{
    // Ambient
    float ambientStrength = 0.2;
    vec3 ambient = ambientStrength * color;

    // Diffuse
    vec3 norm = normalize(Normal);
    float diff = max(dot(norm, -normalize(lightDir)), 0.0);
    vec3 diffuse = diff * color;

    // Specular
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(normalize(lightDir), norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = vec3(0.3) * spec;

    vec3 result = ambient + diffuse + specular;
    FragColor = vec4(result, 1.0);
}
