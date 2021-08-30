using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class DialogGraph : EditorWindow
{
    //BlackBoard
    IMGUIContainer blackboardView;
    SerializedProperty boolConstrainsProperty;
    SerializedProperty floatConstrainsProperty;
    SerializedObject serializedblackboard;
    Blackboard blackboard;



    private TextField fileNameTextField;
    private DialogGraphView _graphView;
    private string _fileName = "";
    private float lastSkreenHight = 0;
    private ListView listView;
    private TextField tittelBar;


    // Fügt Ein Dem Unity ToolBar eine MenuItem Hinzu um das Window Zu öfnen
    [MenuItem("DialogGraph/DialogGraph")]
    public static void OpenDialogGraphWindow()
    {
        var window = GetWindow<DialogGraph>();
        window.titleContent = new GUIContent("Dialog Graph");

    }


    private void Update()
    {
        // Wird in Laufzeit geprüft rund 30/s
        // Wenn sich die anzahl der Nodes in Graphen änderd wird Der listView refrecht
        if (listView.itemsSource.Count != _graphView.nodes.ToList().Count)
        {
            RefrechListView();
        }


        tittelBarDisplayer();

    }
    private void OnEnable()
    {
        GenerateToolbar();
        ConstructView();
        
        _graphView.setDialogGraph(this);
        ConstructUi();

    }


    private void GenerateToolbar()
    {
        this.rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("TestEditorWindow"));
        var toolbar = new Toolbar();
        toolbar.style.height = 30;
        fileNameTextField = new TextField("File Name");
        fileNameTextField.style.marginRight = 20;
        fileNameTextField.style.minWidth = 200;
        fileNameTextField.AddToClassList("Label");
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);

        toolbar.Add(fileNameTextField);
        toolbar.Add(new UnityEngine.UIElements.Button(() => RequestData(true)) { text = "Save" });
        toolbar.Add(new UnityEngine.UIElements.Button(() => RequestData(false)) { text = "Lode" });

        rootVisualElement.contentContainer.Add(toolbar);
    }
    private void ConstructView()
    {
        // Läd die uxml und erzeugt die benutzer Oberfläsche
        VisualElement root = rootVisualElement;
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/DialogEditor/Editor/DialogGraf/TestEditorWindow.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        labelFromUXML.AddToClassList("fullsize");
        root.Add(labelFromUXML);


        //Findet den graphView und bindet an wariable
        _graphView = root.contentContainer.Q<DialogGraphView>();
    }
    private void ConstructUi()
    {
        //SetUp für die restlichen BenutzerElemente
        //Bindet dehn Listview mit der liste
        Debug.Log(rootVisualElement.contentContainer.Q<ListView>().name);
        listView = rootVisualElement.contentContainer.Q<ListView>();
        var items = new List<string>();
        listView.makeItem = () => new Label();
        listView.onItemsChosen += obj => itemSelecktet(obj);
        Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = _graphView.nodes.ToList().ToArray()[i].title;
        listView.style.flexGrow = 1.0f;
        listView.itemHeight = 20;
        listView.bindItem = bindItem;
        Func<VisualElement> makeItem = () => new Label();
        listView.itemsSource = _graphView.nodes.ToList().ToArray();
        tittelBar = rootVisualElement.contentContainer.Q<TextField>("TittelBar");
        tittelBar.RegisterValueChangedCallback<string>(Dialogtittel);


        //Setzt das Blackbord und bindet es an das skriptable objeckt
        blackboard = Resources.Load<Blackboard>($"Blackboard");
        serializedblackboard = new SerializedObject(blackboard);
        blackboardView = rootVisualElement.Q<IMGUIContainer>();
        boolConstrainsProperty = serializedblackboard.FindProperty("boolConstrains");
        floatConstrainsProperty = serializedblackboard.FindProperty("floatConstrains");
        blackboardView.onGUIHandler = () =>{
            //serializedblackboard.Update();
            EditorGUILayout.PropertyField(boolConstrainsProperty);
            EditorGUILayout.PropertyField(floatConstrainsProperty);
            serializedblackboard.ApplyModifiedProperties();
        };
    }

    //Wird von Save-Button und Loade-Button Aufgerufen
    public void RequestData(bool save)
    {
        if (string.IsNullOrEmpty(_fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name", " Please enter file name ", "ok");
            return;
        }
        var saveUtility = GraphSaveUtility.GetInstance(_graphView);
        if (save)
        {
            saveUtility.SaveGraph(_fileName);
        }
        else
        {
            saveUtility.LoadGraph(_fileName);
        }
    }

    public void setFielname(String s)
    {
        _fileName = s;
        fileNameTextField.value = _fileName;
    }

    public void itemSelecktet(IEnumerable<object> obj)
    {
        //Zentriert Das In Der Liste selectierte item in GraphWiew und wählt es an
        var pos = new Vector3(-((Node)listView.selectedItem).GetPosition().x, -((Node)listView.selectedItem).GetPosition().y, 0f);
        pos *= _graphView.scale;
        pos += new Vector3(this.position.width / 4, this.position.height / 2, 0);
        _graphView.viewTransform.position = pos;
     
        _graphView.ClearSelection();
        _graphView.AddToSelection((Node)obj.First());
    }
    public void RefrechListView()
    {
        listView.itemsSource = _graphView.nodes.ToList().ToArray();
    }
    private void tittelBarDisplayer()
    {
        if (_graphView.selection.Count == 1)
        {
            var style = tittelBar.style.display;
            style = DisplayStyle.Flex;
            tittelBar.style.display = style;
        }
        else
        {
            var style = tittelBar.style.display;
            style = DisplayStyle.None;
            tittelBar.style.display = style;
        }
    }
    // Überschreibt dehn Titel der Node
    private void Dialogtittel(ChangeEvent<string> newstring){
        ((Node)_graphView.selection.First()).title = newstring.newValue;
        RefrechListView();
    }


}
