#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Board))]
public class BoardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Board board = (Board)target;

        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Spawn Matrix");

        if (board.spawnMatrix == null ||
            board.spawnMatrix.Length != board.Height)
        {
            if (GUILayout.Button("Init Spawn Matrix"))
            {
                board.spawnMatrix = new BoolRow[board.Height];
                for (int y = 0; y < board.Height; y++)
                {
                    board.spawnMatrix[y] = new BoolRow();
                    board.spawnMatrix[y].cols = new bool[board.Width];
                }
                EditorUtility.SetDirty(board);
            }
            return;
        }

        for (int y = board.Height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < board.Width; x++)
            {
                board.spawnMatrix[y].cols[x] =
                    EditorGUILayout.Toggle(
                        board.spawnMatrix[y].cols[x],
                        GUILayout.Width(18)
                    );
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed)
            EditorUtility.SetDirty(board);
    }
}
#endif
