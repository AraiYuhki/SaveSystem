using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
using Xeon.SaveSystem;

public class SaveFileEditor : EditorWindow
{
    private const int KeyCharacterCount = 32;
    private const int IVCharacterCount = 16;

    [MenuItem("Window/Xeon/Tools/SaveFileEditor")]
    public static void Open() => GetWindow<SaveFileEditor>();

    private string filePath = string.Empty;
    private bool isEncrypted = false;

    private string encryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUV";
    private string encryptionIV = "0123456789ABCDEF";

    private string content = "";
    private Vector2 scrollPosition = Vector2.zero;

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            filePath = EditorGUILayout.TextField(filePath);
            if (GUILayout.Button("�c", GUILayout.Width(30)))
            {
                filePath = EditorUtility.OpenFilePanel("�ǂݍ��ރt�@�C����I�����Ă��������B", string.Empty, "dat");
            }
        }
        isEncrypted = EditorGUILayout.Toggle("�Í�������Ă��邩�H", isEncrypted);
        encryptionKey = EditorGUILayout.TextField($"�Í��L�[({KeyCharacterCount}����)", encryptionKey);
        encryptionIV = EditorGUILayout.TextField($"�Í�IV({IVCharacterCount}����)", encryptionIV);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("�ǂݍ���"))
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    Debug.LogError("�t�@�C�����w�肳��Ă��Ȃ����A������܂���ł����B");
                    return;
                }
                if (isEncrypted)
                    content = LoadEncryptedData(filePath);
                else
                    content = LoadPlaneData(filePath);
            }
            if (GUILayout.Button("Json�ŕۑ�"))
            {
                var savePath = EditorUtility.SaveFilePanel("�ۑ����I��", string.Empty, Path.GetFileName(filePath), "dat");
                if (string.IsNullOrEmpty(savePath))
                    return;
                var data = content.Replace("\r", "").Replace("\n", "");
                File.WriteAllText(savePath, data);
            }
            if (GUILayout.Button("�Í������ĕۑ�"))
            {
                var savePath = EditorUtility.SaveFilePanel("�ۑ����I��", string.Empty, Path.GetFileName(filePath), "dat");
                if (string.IsNullOrEmpty(savePath))
                    return;
                var data = Encoding.UTF8.GetBytes(content);
                data = Compressor.Compress(data);
                data = Cryptor.Encrypt(data, encryptionKey, encryptionIV);
                File.WriteAllBytes(savePath, data);
            }
        }
        using var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition);
        content = EditorGUILayout.TextArea(content);
        scrollPosition = scrollView.scrollPosition;
    }

    private string LoadEncryptedData(string filePath)
    {
        var data = File.ReadAllBytes(filePath);
        data = Cryptor.Decrypt(data, encryptionKey, encryptionIV);
        data = Compressor.Decompress(data);
        return FormatJson(Encoding.UTF8.GetString(data));
    }

    private string LoadPlaneData(string filePath)
    {
        var data = File.ReadAllText(filePath);
        return FormatJson(data);
    }

    private string FormatJson(string json)
    {
        try
        {
            var buffer = Encoding.UTF8.GetBytes(json);
            using (var stream = new MemoryStream())
            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, true, true))
            using (var reader = JsonReaderWriterFactory.CreateJsonReader(buffer, XmlDictionaryReaderQuotas.Max))
            {
                writer.WriteNode(reader, true);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return json;
        }
    }
}
