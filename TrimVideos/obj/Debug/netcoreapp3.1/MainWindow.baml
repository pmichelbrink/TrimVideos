   M S B A M L   `   `   `  ����E  ATrimVideos, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:TrimVideos
TrimVideos  5   	   =#clr-namespace:TrimVideos.ConvertersTrimVideos.Converters  5   	       TrimVideos.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:TrimVideos     5   	   4conv#clr-namespace:TrimVideos.Converters     5   	     � Title$  Trim Videos��5	   	   $	��450��6   $	��500��6*    � DataContext 5
      $   TrimVideos.TrimVideosViewModel  5   
   6&    � 	Resources 5      3   -TrimVideos.Converters.InverseBooleanConverter   inverseBool��%   &	          5   
   6'   5      .��5      �  � RowDefinitions 5   
   �� 5      $
��AUTO �6   6,   �� 5      $
��AUTO �6   6,   �� 5      $
��AUTO �6   6,   �� 5      $
��AUTO �6   6,   �� 5      $��* �6   6)   .+�5   
   � $��	5,10,10,5q�6    � ColumnDefinitions 5      �� 5      $
��AUTO �6#   62   �� 5      $��* �6#   6/   �� 5      $
��AUTO �6#   62   .+�5      �� $��5q�6   Source Path:61   �� 5      $��1��6   $��5q�6&   ��61   �� *SourceFolderPath+6U   �� 5       $��2��6   $��Center=�6%   $��Center=�6@   $	��3,0q�6b   ��6p   �� *BrowseSourceCommand+.��6�   ...6�   5!      � 5"   
   $��1��6   $��5,5,10,5q�6    5#      �� 5$      $
��AUTO �6#   62   �� 5%      $��* �6#   6/   �� 5&      $
��AUTO �6#   62   .+�5(      �� $��5q�6   Output Path:61   �� 5)      $��1��6   $��5q�6&   ��61   �� *OutputFolderPath+6U   �� 5*      $��2��6   $��Center=�6%   $��Center=�6@   $	��3,0q�6b   ��6p   �� *BrowseOutputCommand+.��6�   ...6�   5+      �� 5,   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��5,5,10,5q�6;   .+�5-      �� $��5q�6   Trim Beginning:64   �� 5.      $	��1,5q�6   $��15��6I   ��6#   �� *TrimBeginningSeconds+6Y   �� 5/      $��1,5,5,5q�6   sec6.   �� 50      $ 
Horizontal=�6   $��5q�62   .+�51      �� #��["�6   52      �� 53      $��5q�6   	Trim End:6.   �� 54      $	��1,5q�6   $��15��6C   ��6#   �� *TrimEndSeconds+6S   �� 55      $	��1,5q�6   sec6*   56      � 57   
   $��3��6   $��5,5,10,5q�6    58      �� 59      $
��AUTO �6#   62   �� 5:      $
��AUTO �6#   62   �� 5;      $
��AUTO �6#   62   �� 5<      $��* �6#   6/   .+�5>      �� $	��100��6   $
��Left=�6!   $��5q�6<   $��Start��6h   ��6G   �� *StartCommand+{�6x   �� *IsProcessing+ �� 	Converter# [  6�   �� 5?      $��1��6   $	��100��6%   $
��Left=�61   $��5q�6L   $��Cancel��6y   ��6W   �� *CancelCommand+{�6�   �� *IsProcessing+6�   �� 5@      $��2��6   $��5q�6(   $��SemiBold"�63   	Status:6R   �� 5A      $��3��6   $	��1,5q�6(   $��NoWrap=�6p   $��CharacterEllipsis=�6�   ��65   �� *
StatusText+ � ToolTip 6Q   �� *
StatusText+6�   5B      � 5C   
   $��4��6   $��
10,5,10,10q�6   .+�5D      �� $��False��5E      ��5D      �� *CompletedVideos+ �� View 5F      �� 5G      	 �� AllowsColumnReorder$
	 true��6   
 �� ColumnHeaderToolTip$
 Employee Information��5H      .P�5J      �� $��Completed File��5K   )    �� Width$	 250��5L   )    �� DisplayMemberBinding 5J   )   ��  �� Path$ FilePath�5L   )   �� 5N      $	 100��5O   )   $��Time to Trim��5P   )    5N   )   �� $ 
TimeToTrim�5P   )   5Q      5S      5T      5U      5V      