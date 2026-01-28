using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IPopupRequest<T> {
	public void InitializeEquipmentInfo(T obj, PointerEventData eventData);
	public void OpenPopupRequest(T obj, PointerEventData eventData);
	public void ClosePopupRequest(T obj, PointerEventData evenetData);
	public void InitializePopupPosition(RectTransform rect);
}

public interface IPopupTarget<T> {
	public T targetData { get; set; }
}