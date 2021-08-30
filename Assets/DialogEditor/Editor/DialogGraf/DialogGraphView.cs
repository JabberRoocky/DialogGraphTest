using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Toggle = UnityEngine.UIElements.Toggle;

public class DialogGraphView : GraphView
{
    // Fügt die Classe dehm UIBuilder hinzu
    public new class UxmlFactory : UxmlFactory<DialogGraphView, GraphView.UxmlTraits> { }

    private DialogGraph dialogGraph;
    public readonly Vector2 defaultNodeSize = new Vector2(300, 400);
    public DialogGraphView()
    {


        styleSheets.Add(Resources.Load<StyleSheet>("DialogGrid"));

        // Fügt Standert funktionalitätten hinzu 

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());


        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
        AddElement(GenerateStartPoint());
        RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
        RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
    }

    // Erzeugt Die Verschidenen Nodes Und Ihre Ui
    private Port GeneratePort(Node node,Direction portDirection,Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }
    public DialogNode GenerateStartPoint()
    {
        var node = new DialogNode
        {
            title = "SART",
            GUID = Guid.NewGuid().ToString(), DE_DialogText = "Stuff", Enter = true

        };
        var port = GeneratePort(node, Direction.Output);
        port.portName = "Next";
        node.outputContainer.Add(port);

        node.RefreshPorts();
        node.RefreshExpandedState();

        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        node.SetPosition(new Rect(100, 200, 150, 200));
        return node;
    }
    public DialogNode CreateDialogNode(String titel, String de_DialogueText, String eg_DialogueText, Vector2 position)
    {
        var newNode = new DialogNode
        {
            title = titel,
            DE_DialogText = de_DialogueText,
            EG_DialogText = eg_DialogueText,
            GUID = Guid.NewGuid().ToString()

        };
        var inputPort = GeneratePort(newNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        newNode.inputContainer.Add(inputPort);

        var newPortButton = new UnityEngine.UIElements.Button(() =>{ AddChoicePort(newNode); });
        newPortButton.text = "NewOutput";
        newNode.titleButtonContainer.Add(newPortButton);

        var de_textFild = new TextField(string.Empty);
        de_textFild.RegisterCallback((EventCallback<ChangeEvent<string>>)(evt => newNode.DE_DialogText = evt.newValue));
        de_textFild.multiline = true;
        de_textFild.value = newNode.DE_DialogText;
        de_textFild.label = "DE:";
        de_textFild.contentContainer.Q<Label>().ClearClassList();
        de_textFild.contentContainer.Q<Label>().AddToClassList("Label");

        var eg_textFild = new TextField(string.Empty);
        eg_textFild.RegisterCallback((EventCallback<ChangeEvent<string>>)(evt => newNode.EG_DialogText = evt.newValue));
        eg_textFild.multiline = true;
        eg_textFild.value = newNode.EG_DialogText;
        eg_textFild.label = "EG:";
        eg_textFild.contentContainer.Q<Label>().ClearClassList();
        eg_textFild.contentContainer.Q<Label>().AddToClassList("Label");


        newNode.mainContainer.Add(de_textFild);
        newNode.mainContainer.Add(eg_textFild);
        newNode.mainContainer.AddToClassList("BackRaund");
        newNode.RefreshPorts();
        newNode.RefreshExpandedState();
        newNode.SetPosition(new Rect(position , defaultNodeSize));


        return newNode;


    }
    public CheckNode CreateCheckNode(Vector2 position , CheckNodeData nodeData = null)
    {


        var node = new CheckNode {
            GUID = Guid.NewGuid().ToString(),
            boolConstrains = new List<BoolConstrain>(),
            floatConstrains = new List<FloatConstrain>()
        };
        node.titleContainer.Q()[0].style.paddingRight = 10;
        node.title = "Check-Node";
        node.titleButtonContainer.AddToClassList("CheckNode");
        node.mainContainer.AddToClassList("CheckNode");
        Button switchModeButton = new Button();
        switchModeButton.text = "Switchmode";
        switchModeButton.clicked += () => SwitchNodeMode(node);

        node.titleButtonContainer.Add(switchModeButton);

        var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Single);
        inputPort.portName = "Input";
        node.inputContainer.Add(inputPort);

        var outputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        node.outputContainer.Add(outputPort);
        

        var boolCheckBox = new Box();
        var newboolCheckButton = new Button();
        newboolCheckButton.text = "NewBoolCheck";
        boolCheckBox.Add(newboolCheckButton);
        newboolCheckButton.clicked += () => AddBoolCheckConstrain(ref node);
        
        var floatCheckBox = new Box();
        var newFloatCheckButton = new Button();
        newFloatCheckButton.text = "NewFloatCheck";
        floatCheckBox.Add(newFloatCheckButton);
        newFloatCheckButton.clicked += () => AddFloatCheckConstrain(ref node);

        node.contentContainer.Add(boolCheckBox);
        node.contentContainer.Add(floatCheckBox);

        // wird beim laden des grafen verwendet um die daten wieder zu setzen
        if(nodeData != null)
        {
            node.title = nodeData.titel;
            node.boolConstrains = nodeData.boolConstrains;
            node.floatConstrains = nodeData.floatConstrains;
            if(nodeData.set)
            {
                SwitchNodeMode(node);
            }
        }
        node.SetPosition(new Rect(position, defaultNodeSize));
        return node;
    }
    public ExecuteNode CreateExecutNode(Vector2 position,string bescheibung,DialogEvent dialogevent = null)
    {
        var node = new ExecuteNode
        {
            Event = dialogevent,
            GUID = Guid.NewGuid().ToString()
        };
        node.title = "Execute-Node";


        var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Single);
        inputPort.portName = "Input";
        node.inputContainer.Add(inputPort);

        var outputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        node.outputContainer.Add(outputPort);

        var beschreibungText = new TextField("Beschreibung : ");
        beschreibungText.value = bescheibung;
        beschreibungText.style.minWidth = 200;
        beschreibungText.contentContainer.Q<Label>().ClearClassList();
        beschreibungText.contentContainer.Q<Label>().AddToClassList("Label");
        beschreibungText.RegisterCallback((EventCallback<ChangeEvent<string>>)(evt => node.beschreibung = evt.newValue));
        node.mainContainer.AddToClassList("Execute");
        
        VisualElement visualElement = new VisualElement();
        visualElement.AddToClassList("Backraund2");
        node.contentContainer.Add(visualElement);
        visualElement.Add(beschreibungText);

        Label eventNameLabel = new Label();
        eventNameLabel.style.minWidth = 200;
        eventNameLabel.style.minHeight = 50;
        eventNameLabel.ClearClassList();
        eventNameLabel.AddToClassList("Label");
        eventNameLabel.text = "Noch Kein Event Jetzt";
        eventNameLabel.name = "eventNameLabel";
        visualElement.Add(eventNameLabel);

        Label pfadNameLabel = new Label();
        pfadNameLabel.style.minWidth = 200;
        pfadNameLabel.style.minHeight = 50;
        pfadNameLabel.ClearClassList();
        pfadNameLabel.AddToClassList("Label");
        pfadNameLabel.text = "Noch Kein Pfad Jetzt";
        pfadNameLabel.name = "eventPfadLabel";
        visualElement.Add(pfadNameLabel);


        Label dragAndDropLabel = new Label("DropItem");
        dragAndDropLabel.ClearClassList();
        dragAndDropLabel.AddToClassList("DragAndDrop");
        visualElement.Add(dragAndDropLabel);
        dragAndDropLabel.RegisterCallback<DragUpdatedEvent>(OnItemUpdateEvent);  
        dragAndDropLabel.RegisterCallback<DragPerformEvent>(e =>  OnItemPerformEvent(e,node));

        if(dialogevent != null)
        {
            eventNameLabel.text = dialogevent.name;
            pfadNameLabel.text = AssetDatabase.GetAssetPath(dialogevent);

        }

        node.SetPosition(new Rect(position, defaultNodeSize));
        return node;
    }
    public void AddBoolCheckConstrain(ref CheckNode checkNode,BoolConstrain boolConstrain = null){

        // Der VisualContainer mit dehn BoolConstrains
        Box box = (Box)checkNode.contentContainer.Children().ToArray()[2];

        BoolConstrain newBoolConstrain;
        if (boolConstrain != null)
        {
            newBoolConstrain = boolConstrain;
        }
        else
        {
            newBoolConstrain = new BoolConstrain();
            checkNode.boolConstrains.Add(newBoolConstrain);
        }
        var tempBox = new Box();

        var textfield = new TextField("Name");
        textfield.RegisterValueChangedCallback((EventCallback<ChangeEvent<string>>)(evt =>  newBoolConstrain.name = evt.newValue));
        textfield.style.minWidth = 300;
        var checkbox = new Toggle();
        checkbox.RegisterValueChangedCallback((EventCallback<ChangeEvent<bool>>)(evt => newBoolConstrain.valu = evt.newValue));
        checkbox.text = "Valu";
        
        tempBox.Add(textfield);
        tempBox.Add(checkbox);
        tempBox.AddToClassList("Box2");

        if(boolConstrain != null)
        {
            textfield.value = boolConstrain.name;
            checkbox.value = boolConstrain.valu;
        }

        box.Add(tempBox);
    }
    public void AddFloatCheckConstrain(ref CheckNode checkNode,FloatConstrain floatConstrain = null)
    {
        Box box = (Box)checkNode.contentContainer.Children().ToArray()[3];
        FloatConstrain newFloatConstrain;
        if (floatConstrain != null)
        {
            newFloatConstrain = floatConstrain;
        }
        else
        {
            newFloatConstrain = new FloatConstrain();
            checkNode.floatConstrains.Add(newFloatConstrain);
        }

        var tempBox = new Box();
        var textfield = new TextField("Name");
        textfield.style.minWidth = 300;
        textfield.RegisterValueChangedCallback((EventCallback<ChangeEvent<string>>)(evt => newFloatConstrain.name = evt.newValue));

        var operatorTextLabel = new Label("Operator");
        operatorTextLabel.text = "<";
        operatorTextLabel.RegisterCallback((EventCallback<ChangeEvent<string>>)(evt => newFloatConstrain.op = evt.newValue));


        var valeLabel = new Label("Valu");
        var floatField = new FloatField();
        floatField.RegisterValueChangedCallback((EventCallback<ChangeEvent<float>>)(evt => newFloatConstrain.valu = evt.newValue));


        var changeOperatorButton = new Button();
        changeOperatorButton.text = "changeOperator";
        changeOperatorButton.clicked += () => ChangeOperator(operatorTextLabel);
        tempBox.AddToClassList("Box2");

        tempBox.Add(textfield);
        tempBox.Add(valeLabel);
        tempBox.Add(floatField);
        tempBox.Add(changeOperatorButton);
        tempBox.Add(operatorTextLabel);

        if(floatConstrain != null)
        {
            textfield.value = floatConstrain.name;
            operatorTextLabel.text = floatConstrain.op;
            floatField.value = floatConstrain.valu;
        }
        box.Add(tempBox);
    }

    // Muss Überschrieben werden Um eine aussage zu treffen welche (input/output)Ports miteinander verknüpft werden können
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach((port) => {
            if (startPort != port && startPort.node != port.node)
                compatiblePorts.Add(port);
        
        });
        return compatiblePorts;
    }
    // Fürgt Einem DialogNode Eine Neue Antwortmöglichkeit hinzu
    public void AddChoicePort(Node dialogNode,string de_text ="", string eg_text = "")
    {
        var newPort = GeneratePort(dialogNode, Direction.Output);
        var outputPortCount = dialogNode.outputContainer.Query("connector").ToList().Count;

        var oldLable = newPort.contentContainer.Q<Label>("type");
        oldLable.text = "  ";

        var de_textField = new TextField
        {
            name = string.Empty,
            value = de_text
        };
        de_textField.label = "DE:";
        de_textField.contentContainer.Q<Label>().ClearClassList();
        de_textField.contentContainer.Q<Label>().AddToClassList("Label2");

        var eg_textField = new TextField
        {
            name = string.Empty,
            value = eg_text
        };
        eg_textField.label = "EG:";
        eg_textField.contentContainer.Q<Label>().ClearClassList();
        eg_textField.contentContainer.Q<Label>().AddToClassList("Label2");
        var deleteButton = new UnityEngine.UIElements.Button(() => RemovePort(dialogNode, newPort))
        {
            text = "X"
        };




        Box box = new Box();
        box.AddToClassList("Box");
        box.Add(de_textField);
        box.Add(eg_textField);
        newPort.contentContainer.Add(box);
        newPort.contentContainer.Add(deleteButton);

        newPort.AddToClassList("Port");
        dialogNode.outputContainer.Add(newPort);
        dialogNode.outputContainer.ClearClassList();
        dialogNode.RefreshPorts();
        dialogNode.RefreshExpandedState();
        oldLable.text = "  ";
        foreach (var cont in dialogNode.outputContainer.Children())
        {
            Debug.Log(cont);
        }

    }
    // Überschreibt das defoult ContextMenu das bei Rechtsklik aufgerufen wird
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        //base.BuildContextualMenu(evt);
        {
            var mouspos = evt.localMousePosition;
            var types = TypeCache.GetTypesDerivedFrom<GuidNode>();
            foreach(var type in types)
            {
                evt.menu.AppendAction($"{type.Name}",(a) => CreateNode(type, mouspos));
                
            }
        }
    }
    void CreateNode(System.Type type,Vector2 mauspos)
    {
        Debug.Log(mauspos);
        var positio = mauspos * (Vector2.one / (Vector2)viewTransform.scale) - (Vector2)this.viewTransform.position * (Vector2.one / (Vector2)viewTransform.scale);
        if (type.Equals(typeof(DialogNode)))
        {

            AddElement(CreateDialogNode("DialogNode", "", "", positio));
        }
        else if (type.Equals(typeof(CheckNode)))
        {
            AddElement(CreateCheckNode(positio));
        }
        else if (type.Equals(typeof(ExecuteNode)))
        {
            AddElement(CreateExecutNode(positio,""));
        }
    }
    private void RemovePort(Node dialogNode,Port port)
    {
        
        
        if (port.connected)
        {
            Edge edge = port.connections.First();
            edge.input.Disconnect(edge);
            RemoveElement(edge);
        }


        dialogNode.outputContainer.Remove(port);
        dialogNode.RefreshPorts();
        dialogNode.RefreshExpandedState();


    }
    public void setDialogGraph(DialogGraph dialogGraph)
    {
        this.dialogGraph = dialogGraph;
    }
    private void ChangeOperator(Label label){
        if(label.text == "<")
        {
            label.text = ">";
        }
        else if(label.text == ">")
        {
            label.text = "=";
        }
        else
        {
            label.text = "<";
        }
    }
    private void SwitchNodeMode(CheckNode checkNode)
    {
        if(checkNode.set)
        {
            checkNode.set = false;
            checkNode.title = "Check-Node";
            checkNode.titleButtonContainer.ClearClassList();
            checkNode.mainContainer.RemoveFromClassList("SetNode");
            checkNode.titleButtonContainer.AddToClassList("CheckNode");
            checkNode.mainContainer.AddToClassList("CheckNode");
        }
        else
        {
            checkNode.set = true;
            checkNode.title = "Set-Node";
            checkNode.titleButtonContainer.ClearClassList();
            checkNode.mainContainer.RemoveFromClassList("CheckNode");
            checkNode.titleButtonContainer.AddToClassList("SetNode");
            checkNode.mainContainer.AddToClassList("SetNode");
        }

    }
    void OnItemUpdateEvent(DragUpdatedEvent e){
        if( TypeCache.GetTypesDerivedFrom<DialogEvent>().Contains(DragAndDrop.objectReferences[0].GetType()))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            DragAndDrop.AcceptDrag();
        }
    }
    void OnItemPerformEvent(DragPerformEvent e, ExecuteNode executeNode){
        if (TypeCache.GetTypesDerivedFrom<DialogEvent>().Contains(DragAndDrop.objectReferences[0].GetType())){          
            executeNode.Event = (DialogEvent)DragAndDrop.objectReferences[0];
            executeNode.contentContainer.Q<Label>("eventNameLabel").text = DragAndDrop.objectReferences[0].name;
            executeNode.contentContainer.Q<Label>("eventPfadLabel").text = AssetDatabase.GetAssetPath(executeNode.Event);
            
        }

    }
    void OnDragPerformEvent(DragPerformEvent e) {
        if (DragAndDrop.objectReferences[0].GetType() == typeof(GrafItem))
        {
            dialogGraph.setFielname(DragAndDrop.objectReferences[0].name);
            dialogGraph.RequestData(false);
        }
    }
    void OnDragUpdatedEvent(DragUpdatedEvent e)
    {
        if (DragAndDrop.objectReferences[0].GetType() == typeof(GrafItem))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            DragAndDrop.AcceptDrag();
        }

    }
    
}
