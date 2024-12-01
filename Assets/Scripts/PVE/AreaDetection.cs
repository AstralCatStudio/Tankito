using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaDetection : MonoBehaviour
{
    [SerializeField] string subjectTag;
    public delegate void SubjectDetected(GameObject gameObject);
    public event SubjectDetected OnSubjectDetected;
    public delegate void SubjectDissapear(GameObject gameObject);
    public event SubjectDissapear OnSubjectDissapear;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == subjectTag)
        {
            OnSubjectDetected?.Invoke(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == subjectTag)
        {
            OnSubjectDissapear?.Invoke(collision.gameObject);
        }
    }
}
