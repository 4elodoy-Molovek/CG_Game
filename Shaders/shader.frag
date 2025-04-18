#version 330 core

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

uniform sampler2D texture_diffuse;
uniform vec3 lightPos;
uniform vec3 viewPos;

out vec4 FragColor;

void main()
{
    vec3 color = texture(texture_diffuse, TexCoord).rgb;

    // Простое освещение
    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(Normal, lightDir), 0.0);
    vec3 diffuse = diff * color;

    FragColor = vec4(diffuse, 1.0);
}
