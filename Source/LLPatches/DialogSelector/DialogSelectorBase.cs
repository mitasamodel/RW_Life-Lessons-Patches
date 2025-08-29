using LLPatches.RW_JustUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LLPatches
{
	public abstract class DialogSelectorBase : Window
	{
		// Helper variables.
		private readonly float rowHeight = Utils_GUI.rowHeight;
		private readonly float scrollW = Utils_GUI.scrollWidth;

		public override Vector2 InitialSize => new Vector2(500f, 600f);

		private List<DialogSelectorRow> _filteredList;
		private string _search = "";

		// "Action" or "delegate" is basically a pointer to a function (in C).
		private Action<string> _onSelect;     // Executes method, stored in _onSelect, with 1 argument.
		private Action<Vector2> _onCloseScroll;
		private Vector2 _scroll;
		private Rect? _anchorScreenRect;    // Screen-space rect of the control that opened this dialog (optional).
		private readonly List<DialogSelectorRow> _inputList;

		public DialogSelectorBase(List<DialogSelectorRow> inputList)
			: this(inputList, null, Vector2.zero) { }
		public DialogSelectorBase(List<DialogSelectorRow> inputList, Action<string> onSelect)
			: this(inputList, onSelect, Vector2.zero) { }
		public DialogSelectorBase(List<DialogSelectorRow> inputList, Action<string> onSelect, Vector2 scroll, Action<Vector2> onCloseScroll = null, Rect? anchorScreenRect = null)
		{
			forcePause = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
			doCloseX = true;

			_inputList = inputList;
			_filteredList = null;
			_onSelect = onSelect;
			_scroll = scroll;
			_onCloseScroll = onCloseScroll;
			_anchorScreenRect = anchorScreenRect;

			UpdateFilter();
		}

		public override void DoWindowContents(Rect inRect)
		{
			// Search field
			Rect searchRect = new Rect(0f, 0f, inRect.width, rowHeight);
			string newSearch = Widgets.TextField(searchRect, _search);
			if (newSearch != _search)
			{
				_search = newSearch;
				UpdateFilter();
			}

			//Scrollable area
			float contentHeight = _filteredList.Count * rowHeight;
			Rect positionRect = new Rect(0f, searchRect.height, inRect.width, inRect.height - searchRect.height);   // Where scroll area located.
			Rect contentRect = new Rect(0f, 0f, positionRect.width - scrollW, contentHeight);     //The content inside the scroll area. Coordinates are internal.
			Widgets.BeginScrollView(positionRect, ref _scroll, contentRect, true);
			float curY = 0f;

			foreach (var item in _filteredList)
			{
				Rect rowRect = new Rect(0, curY, contentRect.width, rowHeight);
				DrawRow(rowRect, item);
				if (Widgets.ButtonInvisible(rowRect))
				{
					_onSelect?.Invoke(item.ReturnId);     // Invoke - call a Method (which is stored in _onSelect)
					Close();
				}

				curY += rowHeight;
			}
			Widgets.EndScrollView();
		}

		private void UpdateFilter()
		{
			_filteredList = _inputList
				.Where(i =>
					string.IsNullOrEmpty(_search) ||
					i.Label.ContainsIgnoreCase(_search) ||
					i.ExtraSearchField.ContainsIgnoreCase(_search)
				)
				.ToList();
		}

		public override void PreClose()
		{
			base.PreClose();
			_onCloseScroll?.Invoke(_scroll);
		}

		// Position the dialog relative to the anchor if provided.
		protected override void SetInitialSizeAndPosition()
		{
			var size = InitialSize;
			if (_anchorScreenRect is Rect r)
			{
				float x = r.x + 18f;
				float y = r.y + 18f;

				// Clamp to screen
				x = Mathf.Clamp(x, 0f, UI.screenWidth - size.x);
				y = Mathf.Clamp(y, 0f, UI.screenHeight - size.y);

				windowRect = new Rect(x, y, size.x, size.y);
				return;
			}
			else
			{
				// Fallback to default centering
				base.SetInitialSizeAndPosition();
			}
		}

		protected virtual void DrawRow(Rect inRect, DialogSelectorRow item)
		{
			Widgets.DrawHighlightIfMouseover(inRect);
			Widgets.Label(inRect, item.Label);
		}
	}
}
