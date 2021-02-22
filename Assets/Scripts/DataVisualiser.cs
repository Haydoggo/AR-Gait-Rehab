using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TimedGait
{
    public class DataVisualiser : MonoBehaviour
    {
        public List<Graph> graphs;
        public List<Graph> pinnedGraphs = new List<Graph>();
        public MotionProcessor motionProcessor;

        public Material defaultMaterial;
        public Material selectedMaterial;
        public Material pinnedMaterial;

        
        private int currentGraphIndex;
        private Graph currentGraph;

        public void Awake()
        {
            currentGraphIndex = 0;
            currentGraph = graphs[currentGraphIndex];
            foreach (Graph graph in graphs)
            {
                graph.gameObject.SetActive(false);
            }
            SwitchToGraph(currentGraphIndex);
        }

        public void Update()
        {
            if (motionProcessor.allBuffers.Last().Count > 10)
            {
                graphs[1].WriteValue(motionProcessor.Reading.Value);
                graphs[2].WriteValue(motionProcessor.ReadingDelayed.Value);
                graphs[0].WriteValue(motionProcessor.deltaReading.Value);
                Vector3 vel = motionProcessor.VelocityBuffer[0];
                float latVel = new Vector2(vel.x, vel.z).magnitude;
                graphs[3].WriteValue(latVel);
                graphs[4].WriteValue(motionProcessor.PositionBuffer[0].y);
            }
        }

        public void NextGraph(int numToSkip)
        {
            SwitchToGraph((currentGraphIndex + numToSkip) % graphs.Count);   
        }

        public void SwitchToGraph(int index)
        {
            index = index % graphs.Count;
            if (!pinnedGraphs.Contains(currentGraph))
            {
                currentGraph.gameObject.SetActive(false);
            }
            currentGraph.label.gameObject.SetActive(false);
            currentGraph.SetBarMaterial(defaultMaterial);

            currentGraphIndex = index;
            currentGraph = graphs[currentGraphIndex];

            currentGraph.gameObject.SetActive(true);
            currentGraph.label.gameObject.SetActive(true);
            if (pinnedGraphs.Contains(currentGraph))
            {
                currentGraph.SetBarMaterial(pinnedMaterial);
            }
            else
            {
                currentGraph.SetBarMaterial(selectedMaterial);
            }
        }

        public void PinGraph()
        {
            if (pinnedGraphs.Contains(currentGraph))
            {
                pinnedGraphs.Remove(currentGraph);
            } 
            else
            {
                pinnedGraphs.Add(currentGraph);
            }
            SwitchToGraph(currentGraphIndex);
        }

        public void SetGraphScale(float scale)
        {
            currentGraph.yScale = scale;
        }

        public void SetGraphAlpha(float alpha)
        {
            currentGraph.alpha = alpha;
        }
    }
}