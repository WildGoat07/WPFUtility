# WPFUtility

A bunch of useful stuff to work with in a WPF app

## Features

### ViewModel bases

You can easily create a ViewModel class by inheriting `ViewModelBase`, very similar to the `DependencyProperty`, with the addition to have the editable and validation handled :

```csharp
class PersonViewModel : ViewModelBase
{
    // Some properties
    public int Age { get => (int)GetValue(nameof(Age)); set => SetValue(nameof(Age), value); }

    public string Name { get => (string)GetValue(nameof(Name)); set => SetValue(nameof(Name), value); }

    public string Surname { get => (string)GetValue(nameof(Surname)); set => SetValue(nameof(Surname), value); }

    //This stores more informations about the diffrent properties, in case a property needs more control
    protected override Dictionary<string, PropertyDetails> SpecialProperties => new()
    {
        {
            nameof(Age), // we add a special case for the age
            new PropertyDetails(
                defaultValue: 34, // Age is at 34 by default
                validationRule: newAge => // Verification that age is above or equal to 0
                {
                    if ((int)newAge < 0)
                        return new[] { new Error(0, "Age can't go under 0 !") };
                    else
                        return null;
                })
        }
    };
}
```

We can then have this XAML code in our MainWindow :

```xaml
<StackPanel>
    <StackPanel.DataContext>
        <local:PersonViewModel />
    </StackPanel.DataContext>
    <Slider
        Minimum="-2"
        Maximum="99"
        Value="{Binding Path=Age}" />
    <TextBox
        Text="{Binding Path=Age}" />
</StackPanel>
```

Output :

![image](https://user-images.githubusercontent.com/30344403/132067716-50346d8d-bd6f-44d4-80c7-9bfb11fda918.png)
![image](https://user-images.githubusercontent.com/30344403/132067748-2d1dd7bf-b312-4bcf-b414-e194d9d4ae76.png)

There is also another view model base, for more niche cases, that listen to its `INotifyPropertyChanged` sub properties, that retriggers them.

For example, imagine two ViewModel, `PersonViewModel` and `CityViewModel`, where `PersonViewModel` contains `CityViewModel` under the `City` property. Any change to that city will trigger a `PropertyChanged` on the person, with the name `City.Name` for the property, if the name of the city changed.

### Collections

The library contains many different collection view fully compatible with WPF. Each take a base collection (it can be an `ObservableCollection` or another collection view).

But why ? Because using only the base collection view, only one can be kinda great, the `ListCollectionView` since it can sort and filter easily. But this one takes an `IList`, but is not an `IList` itself ! With this lib, you can chain multiple view and they provide more utility.

- `FilteredCollectionView` : Filters the output view based on the source.
- `SortedCollectionView` : Sorts the output view based on the source.
- `GroupCollectionView` : Groups the items from the source based on a predicate.
- `ExtendableCollectionView` : A collection view that can extend its content by adding virtual items based on the actual items. Kinda like a tree view, but compatible with `DataGrid` for example.
- and more !

### Controls

There is only 2 controls for now :

`ActionIcon` : used to create customized button icons by giving it attributes that change the final render.

For example, we got this resource : **folder.png** ![FolderClosed_16x](https://user-images.githubusercontent.com/30344403/132068508-0a16df61-74fc-4139-8cdd-cf57206d7581.png)

We can create custom icons with this :

```xaml
<uc:ActionIcon
    HorizontalAlignment="Left"
    Margin="5"
    BaseImage="/folder.png"
    Action="ADD" />
```

Ouput : ![image](https://user-images.githubusercontent.com/30344403/132068812-01cd5be7-2b29-4bcf-9395-b99798a82603.png)

Highly customizable with many icons, with custom images or controls.

Another control is the `DropdownButton` for obvious reasons, this really should be in WPF in my opinion :

```xaml
<uc:DropdownButton
    ShowDefaultButton="True">
    <uc:DropdownButton.Dropdown>
        <ContextMenu>
            <MenuItem
                Header="Add folder">
                <MenuItem.Icon>
                    <uc:ActionIcon
                        BaseImage="/folder.png"
                        Action="ADD" />
                </MenuItem.Icon>
            </MenuItem>
        </ContextMenu>
    </uc:DropdownButton.Dropdown>
    <Label
        Content="Create folder" />
</uc:DropdownButton>
```

Output : ![image](https://user-images.githubusercontent.com/30344403/132069056-148711b4-b138-48e6-80a2-13c7b2cac8f6.png)
