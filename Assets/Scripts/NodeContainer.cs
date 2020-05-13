public class NodeContainer : ElementContainer
{
	protected override void OnBeginDrag()
	{
		BringToFront();
	}

	protected override void OnDrag()
	{
		NodePanel panel = Panel as NodePanel;
		
		if (panel != null)
			panel.DragContainer(this);
	}

	protected override void OnEndDrag()
	{
		NodePanel panel = Panel as NodePanel;
		
		if (panel != null)
			panel.DropContainer(this);
	}

	protected override void OnPointerDown()
	{
	}
}