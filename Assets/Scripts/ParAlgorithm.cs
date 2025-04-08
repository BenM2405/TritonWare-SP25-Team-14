using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ParAlgorithm : MonoBehaviour
{
    [SerializeField] private PuzzleManager puzzleManager;
    private TileCopy[,] startState, goalState;
    private List<TileCopy[,]> visitedStates = new List<TileCopy[,]>();
    private Queue<Board> boardQueue = new Queue<Board>();
    private int MAX_TURNS = 20;
    private System.Random rnd;
    private int stateChecks;

    void Awake()
    {
        rnd = new System.Random();
    }

    public int CalculatePar()
    {
        stateChecks = 0;
        if (puzzleManager == null)
        {
            Debug.LogError("Puzzle Manager not found! Can not perform par algorithm.");
            return -1;
        }
        startState = copyState(puzzleManager.tiles);
        goalState = copyState(puzzleManager.targetTiles);
        Debug.Log("Start board: ");
        printState(startState);
        Debug.Log("Goal board: ");
        printState(goalState);

        boardQueue.Enqueue(new Board(startState, 0));
        visitedStates.Add(startState);

        while (boardQueue.Count != 0)
        {
            Board currentBoard = boardQueue.Dequeue();
            Debug.Log("Queue Length: " + boardQueue.Count);
            Debug.Log("Current moves: " + currentBoard.numMoves);
            Debug.Log("Current board: ");
            printState(currentBoard.state);
            stateChecks++;
            if (currentBoard.Matches(goalState))
            {
                printPerformanceStats();
                return currentBoard.numMoves;
            }
            if (currentBoard.numMoves > MAX_TURNS)
            {
                return -1;
            }

            for (int i = 0; i < currentBoard.state.GetLength(0); i++)
            {
                for (int j = 0; j < currentBoard.state.GetLength(1); j++)
                {
                    testNeighborsAtGivenStart(currentBoard, new Vector2Int(i, j));
                }
            }
        }

        return -1;
    }

    private void testNeighborsAtGivenStart(Board currentBoard, Vector2Int startSwapPos)
    {
        foreach (TileCopy[,] neighbor in GetNeighbors(currentBoard.state, startSwapPos))
        {
            Debug.Log("Now testing Neighbor!");
            printState(neighbor);
            Board neighborBoard = new Board(neighbor, currentBoard.numMoves + 1);
            bool matchFound = false;
            foreach (TileCopy[,] visitedState in visitedStates)
            {
                stateChecks++;
                if (neighborBoard.Matches(visitedState)) { Debug.Log("Neighbor has already been visited!"); matchFound = true; break; }
            }
            if (!matchFound)
            {
                visitedStates.Add(neighbor);
                boardQueue.Enqueue(neighborBoard);
                Debug.Log("Added neighbor to queue!");
            }
        }
    }

    private List<TileCopy[,]> GetNeighbors(TileCopy[,] currentState, Vector2Int startSwapPos)
    {
        Debug.Log("Getting neighbors!");
        List<TileCopy[,]> neighbors = new List<TileCopy[,]>();
        Vector2Int[] possibleMoves = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        Debug.Log("Getting neighbors from " + startSwapPos);
        foreach (Vector2Int possibleMove in possibleMoves)
        {
            TileCopy[,] newState = Swap(currentState, possibleMove, startSwapPos);
            if (newState != null)
            {
                neighbors.Add(newState);
            }
        }

        return neighbors;
    }

    private TileCopy[,] Swap(TileCopy[,] currentState, Vector2Int possibleMove, Vector2Int startSwapPos)
    {
        TileCopy[,] newState = new TileCopy[currentState.GetLength(0), currentState.GetLength(1)];
        for (int i = 0; i < newState.GetLength(0); i++)
        {
            for (int j = 0; j < newState.GetLength(1); j++)
            {
                newState[i, j] = new TileCopy(currentState[i, j].currentSymbol);
            }
        }
        Vector2Int targetSwapPos = new Vector2Int(startSwapPos.x + possibleMove.x, startSwapPos.y + possibleMove.y);
        if (targetSwapPos.x >= newState.GetLength(0) || targetSwapPos.y >= newState.GetLength(1) || targetSwapPos.x < 0 || targetSwapPos.y < 0) { return null; }
        //Debug.Log("Target Swap Position: " + targetSwapPos);
        TileCopy tileA = newState[startSwapPos.x, startSwapPos.y];
        TileCopy tileB = newState[targetSwapPos.x, targetSwapPos.y];

        var tempType = tileA.currentSymbol;


        tileA.currentSymbol = tileB.currentSymbol;
        tileB.currentSymbol = tempType;

        //printState(newState);
        return newState;
    }

    private void printState(TileCopy[,] state)
    {
        for (int i = 0; i < state.GetLength(0); i++)
        {
            String output = "";
            for (int j = 0; j < state.GetLength(1); j++)
            {
                output += state[i, j].currentSymbol + " ";
            }
            Debug.Log(output);
        }
    }

    private TileCopy[,] copyState(Tile[,] state)
    {
        TileCopy[,] copy = new TileCopy[state.GetLength(0), state.GetLength(1)];
        for (int i = 0; i < copy.GetLength(0); i++)
        {
            for (int j = 0; j < copy.GetLength(1); j++)
            {
                copy[i, j] = new TileCopy(state[i, j]);
            }
        }

        return copy;
    }

    private void printPerformanceStats()
    {
        Debug.Log("State comparisons (Checking Neighbors & Goal): " + stateChecks);
        Debug.Log("States visited: " + visitedStates.Count);
    }

    private class Board
    {
        public TileCopy[,] state;
        public int numMoves;

        public Board(TileCopy[,] state, int numMoves)
        {
            this.state = state;
            this.numMoves = numMoves;
        }

        public bool Matches(TileCopy[,] otherState)
        {
            for (int i = 0; i < state.GetLength(0); i++)
            {
                for (int j = 0; j < state.GetLength(1); j++)
                {
                    if (state[i, j].currentSymbol != otherState[i, j].currentSymbol)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    private class TileCopy
    {
        public String currentSymbol;

        public TileCopy(Tile tile)
        {
            currentSymbol = tile.currentSymbol.ToString();
        }
        public TileCopy(String symbol)
        {
            currentSymbol = symbol;
        }
    }
}
