using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services
{
    public class SettingsService
    {
        private readonly RestaurantDbContext _dbContext;
        private Dictionary<string, string> _cachedSettings;
        
        // Default settings if not found in the database
        private readonly Dictionary<string, string> _defaultSettings = new()
        {
            // Threshold amount for free delivery
            ["MinimumOrderAmountForFreeDelivery"] = "75",
            
            // Delivery cost for orders below threshold
            ["DeliveryCost"] = "15",
            
            // Threshold amount for discount
            ["DiscountThresholdAmount"] = "100",
            
            // Discount percentage
            ["DiscountPercentage"] = "10",
            
            // Number of orders required in a time period for loyalty discount
            ["LoyaltyOrderCount"] = "3",
            
            // Time period (in days) for loyalty discount
            ["LoyaltyTimePeriodDays"] = "30",
            
            // Loyalty discount percentage
            ["LoyaltyDiscountPercentage"] = "15"
        };
        
        public SettingsService()
        {
            _dbContext = new RestaurantDbContext();
            _cachedSettings = new Dictionary<string, string>();
            _ = LoadSettingsAsync();
        }
        
        public async Task LoadSettingsAsync()
        {
            try
            {
                var settings = await _dbContext.Settings.ToListAsync();
                _cachedSettings.Clear();
                
                foreach (var setting in settings)
                {
                    _cachedSettings[setting.SettingKey] = setting.SettingValue;
                }
                
                // Ensure all default settings exist
                foreach (var defaultSetting in _defaultSettings)
                {
                    if (!_cachedSettings.ContainsKey(defaultSetting.Key))
                    {
                        await SetSettingAsync(defaultSetting.Key, defaultSetting.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                
                // Use default settings if database fails
                _cachedSettings = new Dictionary<string, string>(_defaultSettings);
            }
        }
        
        public async Task<string> GetSettingAsync(string key)
        {
            // If not in cache, try to load from database
            if (!_cachedSettings.ContainsKey(key))
            {
                var setting = await _dbContext.Settings.FindAsync(key);
                if (setting != null)
                {
                    _cachedSettings[key] = setting.SettingValue;
                }
                else if (_defaultSettings.ContainsKey(key))
                {
                    // Use default if available
                    await SetSettingAsync(key, _defaultSettings[key]);
                    _cachedSettings[key] = _defaultSettings[key];
                }
                else
                {
                    return null;
                }
            }
            
            return _cachedSettings[key];
        }
        
        public string GetSetting(string key)
        {
            if (_cachedSettings.ContainsKey(key))
            {
                return _cachedSettings[key];
            }
            
            if (_defaultSettings.ContainsKey(key))
            {
                return _defaultSettings[key];
            }
            
            return null;
        }
        
        public async Task SetSettingAsync(string key, string value)
        {
            var setting = await _dbContext.Settings.FindAsync(key);
            
            if (setting != null)
            {
                setting.SettingValue = value;
                _dbContext.Settings.Update(setting);
            }
            else
            {
                setting = new Setting
                {
                    SettingKey = key,
                    SettingValue = value
                };
                _dbContext.Settings.Add(setting);
            }
            
            await _dbContext.SaveChangesAsync();
            _cachedSettings[key] = value;
        }
        
        // Helper methods for specific settings
        
        public decimal GetMinimumOrderAmountForFreeDelivery()
        {
            return decimal.Parse(GetSetting("MinimumOrderAmountForFreeDelivery") ?? "75");
        }
        
        public decimal GetDeliveryCost()
        {
            return decimal.Parse(GetSetting("DeliveryCost") ?? "15");
        }
        
        public decimal GetDiscountThresholdAmount()
        {
            return decimal.Parse(GetSetting("DiscountThresholdAmount") ?? "100");
        }
        
        public decimal GetDiscountPercentage()
        {
            return decimal.Parse(GetSetting("DiscountPercentage") ?? "10");
        }
        
        public int GetLoyaltyOrderCount()
        {
            return int.Parse(GetSetting("LoyaltyOrderCount") ?? "3");
        }
        
        public int GetLoyaltyTimePeriodDays()
        {
            return int.Parse(GetSetting("LoyaltyTimePeriodDays") ?? "30");
        }
        
        public decimal GetLoyaltyDiscountPercentage()
        {
            return decimal.Parse(GetSetting("LoyaltyDiscountPercentage") ?? "15");
        }
    }
} 