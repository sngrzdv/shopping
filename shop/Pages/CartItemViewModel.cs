using shop.AppData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CartItemViewModel : INotifyPropertyChanged
{
    private readonly basket _basketItem;

    public CartItemViewModel(basket basketItem)
    {
        _basketItem = basketItem;
    }

    public product product => _basketItem.product;
    public int quantity
    {
        get => _basketItem.quantity;
        set
        {
            _basketItem.quantity = value;
            OnPropertyChanged(nameof(quantity));
            OnPropertyChanged(nameof(ItemTotal));
        }
    }

    public decimal ItemTotal => _basketItem.product.price * _basketItem.quantity;

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}