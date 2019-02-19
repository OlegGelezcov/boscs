using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PromoCodesEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TCPromoCodesEntities _ctx;

        public MainWindow()
        {
            _ctx = new TCPromoCodesEntities();
            InitializeComponent();
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            loader.Visibility = Visibility.Visible;
            dgData.ItemsSource = await _ctx.PromoCodes.ToListAsync();
            loader.Visibility = Visibility.Collapsed;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _ctx.Dispose();
        }

        private async void dgData_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                loader.Visibility = Visibility.Visible;

                var dc = e.Row.DataContext as PromoCode;
                var q = await _ctx.PromoCodes.FirstOrDefaultAsync(x => x.Id == dc.Id);
                if (q != null)
                {
                    FillPromoCodeData(dc, q);

                    await _ctx.SaveChangesAsync();
                }
                else
                {
                    q = new PromoCode();
                    FillPromoCodeData(dc, q);

                    _ctx.PromoCodes.Add(q);
                    await _ctx.SaveChangesAsync();
                }


                dgData.ItemsSource = await _ctx.PromoCodes.ToListAsync();

                loader.Visibility = Visibility.Collapsed;
            }
        }

        private static void FillPromoCodeData(PromoCode dc, PromoCode q)
        {
            q.IsMultipleRedeemable = dc.IsMultipleRedeemable;
            q.Code = dc.Code;
            q.ExpirationDate = dc.ExpirationDate;
            q.MaxRedeems = dc.MaxRedeems;
            q.ObjectType = dc.ObjectType;
            q.Value = dc.Value;
        }

        private async void btnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var val = dgData.SelectedValue as PromoCode;

            var result = MessageBox.Show("Are you sure you want to delete this?", "Deletion warning", MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
            if (result == MessageBoxResult.Yes)
            {
                loader.Visibility = Visibility.Visible;

                var q = await _ctx.PromoCodes.FirstOrDefaultAsync(x => x.Id == val.Id);
                var children = await _ctx.RedeemedCodes.Where(x => x.PromoCodeId == q.Id).ToListAsync();
                foreach (var x in children)
                {
                    _ctx.RedeemedCodes.Remove(x);
                }
                _ctx.PromoCodes.Remove(q);

                await _ctx.SaveChangesAsync();


                dgData.ItemsSource = await _ctx.PromoCodes.ToListAsync();


                loader.Visibility = Visibility.Collapsed;
            }
        }
    }
}
