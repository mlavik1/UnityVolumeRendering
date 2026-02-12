using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{
    public class ResizableArea
    {
        private Rect rect;
        private Rect leftBorderRect, rightBorderRect, topBorderRect, bottomBorderRect;
        private Rect[] visibleBorderRects = new Rect[4];

        private const int BORDER_SIZE = 4;
        private const int VISIBLE_BORDER_SIZE = 1;

        private bool moveLeftBorder = false;
        private bool moveRightBorder = false;
        private bool moveTopBorder = false;
        private bool moveBottomBorder = false;
        private Vector2 mouseClickPos = Vector2.zero;
        private Rect rectOnClick;

        public bool rectChanged = false;

        public Rect GetRect()
        {
            return rect;
        }

        public void SetRect(Rect rect)
        {
            this.rect = rect;

            leftBorderRect = new Rect(rect.x, rect.y, BORDER_SIZE, rect.height);
            rightBorderRect = new Rect(rect.x + rect.width - BORDER_SIZE, rect.y, BORDER_SIZE, rect.height);
            topBorderRect = new Rect(rect.x, rect.y, rect.width, BORDER_SIZE);
            bottomBorderRect = new Rect(rect.x, rect.y + rect.height - BORDER_SIZE, rect.width, BORDER_SIZE);

            visibleBorderRects[0] = new Rect(rect.x, rect.y, VISIBLE_BORDER_SIZE, rect.height);
            visibleBorderRects[1] = new Rect(rect.x + rect.width - VISIBLE_BORDER_SIZE, rect.y, VISIBLE_BORDER_SIZE, rect.height);
            visibleBorderRects[2] = new Rect(rect.x, rect.y, rect.width, VISIBLE_BORDER_SIZE);
            visibleBorderRects[3] = new Rect(rect.x, rect.y + rect.height - VISIBLE_BORDER_SIZE, rect.width, VISIBLE_BORDER_SIZE);
        }

        public void Draw()
        {
            foreach (Rect visibleRect in visibleBorderRects)
                EditorGUI.DrawRect(visibleRect, Color.green);

            EditorGUIUtility.AddCursorRect(leftBorderRect, MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(rightBorderRect, MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(topBorderRect, MouseCursor.ResizeVertical);
            EditorGUIUtility.AddCursorRect(bottomBorderRect, MouseCursor.ResizeVertical);
        }

        public bool Intersects(Vector2 mousePos)
        {
            return rect.Contains(mousePos);
        }

        public bool IntersectsBorder(Vector2 mousePos)
        {
            return leftBorderRect.Contains(mousePos)
            || rightBorderRect.Contains(mousePos)
            || topBorderRect.Contains(mousePos)
            || bottomBorderRect.Contains(mousePos);
        }

        public void StartMoving(Vector2 mousePos)
        {
            moveLeftBorder = moveRightBorder = moveTopBorder = moveBottomBorder = false;

            mouseClickPos = mousePos;
            rectOnClick = this.rect;

            if (leftBorderRect.Contains(mousePos))
                moveLeftBorder = true;
            else if (rightBorderRect.Contains(mousePos))
                moveRightBorder = true;
            else if (topBorderRect.Contains(mousePos))
                moveTopBorder = true;
            else if (bottomBorderRect.Contains(mousePos))
                moveBottomBorder = true;
        }

        public void UpdateMoving(Vector2 mousePos)
        {
            Rect oldRect = rectOnClick;
            Rect newRect = rectOnClick;

            Vector2 mouseOffset = mousePos - mouseClickPos;
            if (moveLeftBorder)
            {
                newRect.x += mouseOffset.x;
                newRect.width -= mouseOffset.x;
            }
            else if (moveRightBorder)
            {
                newRect.width += mouseOffset.x;
            }
            else if (moveTopBorder)
            {
                newRect.y += mouseOffset.y;
                newRect.height -= mouseOffset.y;
            }
            else if (moveBottomBorder)
            {
                newRect.height += mouseOffset.y;
            }
            else
            {
                newRect.x += mouseOffset.x;
                newRect.y += mouseOffset.y;
            }

            SetRect(newRect);

            rectChanged = newRect != oldRect;
        }

        public void StopMoving()
        {
            
        }
    }
}
