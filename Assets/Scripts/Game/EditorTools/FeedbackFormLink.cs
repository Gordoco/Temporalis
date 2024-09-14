using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Net.WebRequestMethods;

public class FeedbackFormLink : MonoBehaviour
{
    [SerializeField] string FeedbackFormURL = "https://forms.gle/inrWiuXDAMgMFxsY7";
    [SerializeField] string BugReportFormURL = "https://forms.gle/Jy5asWsrW6RwECBx9";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenFeedbackForm()
    {
        Application.OpenURL(FeedbackFormURL);
    }

    public void OpenBugReportForm()
    {
        Application.OpenURL(BugReportFormURL);
    }
}
