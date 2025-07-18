using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDeleter : MonoBehaviour
{
	public List<Line> lines;

	public void OnTriggerEnter2D(Collider2D other)
	{
		Line line;
		if ((line = other.GetComponent<Line>()) != null)
        {
			if (!lines.Contains(line))
				lines.Add(line);
        }
	}

    public void OnTriggerExit2D(Collider2D other)
    {
        Line line;
        if ((line = other.GetComponent<Line>()) != null)
        {
            if (lines.Contains(line))
                lines.Remove(line);
        }
    }

    public void DeleteLines()
    {
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            Destroy(lines[i].gameObject);
        }
        lines.Clear();
    }

    public void DeleteLinesWithDelay(float delay)
    {
        Invoke(nameof(DeleteLines), delay);
    }
}
