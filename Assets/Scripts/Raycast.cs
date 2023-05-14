using System.Collections;
using System.IO;
using UnityEngine;

public class Raycast : MonoBehaviour
{
    private Ray _ray;
    private Texture2D _texture;

    [Header("Texture")]
    [SerializeField] private Vector2 textureResolution = new(50, 50);
    [SerializeField] private FilterMode textureAntiAliasing = FilterMode.Bilinear;

    [Header("Camera")]
    [SerializeField][Range(1f, 10f)] private float cameraSize = 1f;
    [SerializeField] private Color cameraBackgroundColor = Color.black;
    [SerializeField] private float cameraDistance = 5f;

    [Header("Light")]
    [SerializeField]
    private Transform lightSource;
    [SerializeField][Range(0f, 1f)] private float ambientLight = 0.1f;
    [SerializeField][Range(0f, 100f)] private float specularLight = 10f;

    private void Start()
    {
        _ray = new Ray(transform.position, transform.forward);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetupTexture();
            StartCoroutine("RenderScene");
            SaveTextureAsPNG(_texture, $"Assets/Textures/Texture_{textureResolution.x}x{textureResolution.y}.png");
        }
        Debug.DrawRay(_ray.origin, _ray.direction * cameraDistance, Color.red);
    }

    private void SetupTexture()
    {
        Renderer rend = GetComponent<Renderer>();
        _texture = new Texture2D((int) textureResolution.x, (int) textureResolution.y)
        {
            filterMode = textureAntiAliasing
        };
        rend.material.mainTexture = _texture;
    }

    private IEnumerator RenderScene()
    {
        for (int x = 0; x < _texture.width; x++)
        {
            for (int y = 0; y < _texture.height; y++)
            {
                float cameraSizeHalf = cameraSize * 0.5f;
                float px = (float) x / _texture.width * cameraSize - cameraSizeHalf;
                float py = (float) y / _texture.height * cameraSize - cameraSizeHalf;
                _ray.origin = new Vector3(px, py, 0) + transform.position + new Vector3(0, 0, 0.1f);

                Color c = Physics.Raycast(_ray, out RaycastHit hit)
                    ? BlinnPhongShading(hit)
                    : cameraBackgroundColor;

                _texture.SetPixel(x, y, c);
                _texture.Apply();

                // yield return new WaitForSeconds(0.01f);
            }
        }

        yield return null;
    }

    // private Color PhongShading(RaycastHit hit)
    // {
    //     Vector3 lightDir = (lightSource.position - hit.point).normalized;
    //     float intensity = Mathf.Max(0f,Vector3.Dot(lightDir, hit.normal));
    //     return hit.transform.GetComponent<MeshRenderer>().material.color * intensity;
    // }

    private Color BlinnPhongShading(RaycastHit hit)
    {
        Color hitColor = hit.transform.GetComponent<MeshRenderer>().material.color;

        Vector3 lightDir = (lightSource.position - hit.point).normalized;
        Vector3 viewDir = (transform.position - hit.point).normalized;
        Vector3 halfDir = (lightDir + viewDir).normalized;

        float intensity = Mathf.Max(0f,Vector3.Dot(halfDir, hit.normal));

        Color ambientColor = hitColor * ambientLight;
        Color diffuseColor = hitColor * intensity;
        Color specularColor = Color.white * Mathf.Pow(intensity, specularLight);
        return ambientColor + diffuseColor + specularColor;
    }

    private static void SaveTextureAsPNG(Texture2D texture, string fullPath)
    {
        var dirPath = Application.dataPath + fullPath;
        if (Directory.Exists(dirPath) == false) Directory.CreateDirectory(dirPath);

        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);
    }
}
