﻿using UnityEngine;

[RequireComponent(typeof(Valve.VR.InteractionSystem.Hand), typeof(LineRenderer))]
public class ViveHandInteractionLaserPointer : MonoBehaviour
{
    [SerializeField]
    private string m_hitTag;

    private GameObject m_targetObject;
    private bool m_isHoldingSomething = false;

    private RaycastHit m_raycast;

    private Valve.VR.InteractionSystem.Hand m_hand;
    private LineRenderer m_lineRenderer;



    // ---------- ---------- ---------- ---------- ----------
    void Start ()
    {
        m_hand = GetComponent<Valve.VR.InteractionSystem.Hand>();
        m_lineRenderer = GetComponent<LineRenderer>();
	}


    // ---------- ---------- ---------- ---------- ----------
    void Update ()
    {
        if (CheckObjectHit())
        {
            if (CheckObject())
            {
                if (CheckInput())
                {
                    ObjectInteract();
                }
            }
        }

        UpdateLaser();
    }


    // ---------- ---------- ---------- ---------- ----------
    public bool CheckObjectHit()
    {
        if (Physics.Raycast(m_hand.transform.position, m_hand.transform.forward, out m_raycast))
        {
            if (m_targetObject != m_raycast.transform.gameObject && !m_isHoldingSomething)
            {
                if (m_targetObject != null)
                {
                    Mouledoux.Components.Mediator.instance.NotifySubscribers
                        (m_targetObject.GetInstanceID().ToString() + "->offhighlight", new Mouledoux.Callback.Packet());
                }

                m_targetObject = m_raycast.transform.gameObject;

                Mouledoux.Components.Mediator.instance.NotifySubscribers
                    (m_targetObject.GetInstanceID().ToString() + "->onhighlight", new Mouledoux.Callback.Packet());
            }

            return true;
        }

        m_raycast.point = m_hand.transform.position + m_hand.transform.forward;
        return false;
    }


    // ---------- ---------- ---------- ---------- ----------
    public bool CheckObject()
    {
        return m_targetObject.GetComponent<InteractableObject>() != null;
    }


    // ---------- ---------- ---------- ---------- ----------
    public bool CheckInput()
    {
        return (m_hand.controller.GetHairTriggerDown());
    }


    // ---------- ---------- ---------- ---------- ----------
    public int ObjectInteract()
    {
        Mouledoux.Components.Mediator.instance.NotifySubscribers
            (m_targetObject.GetInstanceID().ToString() + "->oninteract", new Mouledoux.Callback.Packet());

        if (m_targetObject.GetComponent<InteractableObject>().m_pickup != true)
        {
            StartCoroutine(HoldObject());
        }

        return 0;
    }


    // ---------- ---------- ---------- ---------- ----------
    public void UpdateLaser()
    {
        m_lineRenderer.SetPositions( new Vector3[] {m_hand.transform.position, m_raycast.point});
    }


    // ---------- ---------- ---------- ---------- ----------
    public System.Collections.IEnumerator HoldObject()
    {
        Vector3 lastPos = Vector3.zero;
        Transform lastParent = null;

        Transform t = m_raycast.transform;
        Collider c = m_raycast.collider;

        c.enabled = false;

        m_isHoldingSomething = true;

        while (m_hand.controller.GetHairTrigger())
        {
            if (m_raycast.transform.gameObject.tag == t.gameObject.tag)
            {
                lastPos = t.position = m_raycast.point;
                lastParent = m_raycast.transform;
            }

            else
            {
                t.position = lastPos;
            }

            t.parent = lastParent;

            yield return null;
        }
        
        Mouledoux.Components.Mediator.instance.NotifySubscribers
            (t.gameObject.GetInstanceID().ToString() + "->offinteract", new Mouledoux.Callback.Packet());

        c.enabled = true;
        m_isHoldingSomething = false;
    }
}
