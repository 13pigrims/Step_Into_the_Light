using UnityEngine;

public class UIType
{
    string name;
    public string Name { get => name;}
    string path;
    public string Path { get => path; }
    // 构造函数
    public UIType(string ui_path, string ui_name)
    {
        name = ui_name;
        path = ui_path;
    }
}
