(function (window, $) {
    'use strict';

    if (!$) {
        console.error('Seafood Select2 requires jQuery.');
        return;
    }

    window.seafoodSelect2 = {
        init: function (selectId, dotNetRef, options, initialItems) {
            var $select = $('#' + selectId);
            if (!$select.length) return;
            if (!$.fn.select2) {
                console.error('Seafood Select2 requires the Select2 library.');
                return;
            }

            this.fillOptions($select, initialItems || []);
            $select.select2({
                width: '100%',
                allowClear: true,
                placeholder: options?.placeholder || options?.Placeholder || 'Chọn...',
                disabled: Boolean(options?.disabled ?? options?.Disabled),
                minimumInputLength: 0,
                dropdownParent: $select.closest('.modal').length ? $select.closest('.modal') : $(document.body),
                ajax: {
                    delay: 250,
                    cache: true,
                    transport: function (params, success, failure) {
                        var term = params.data?.term || '';
                        var page = params.data?.page || 1;
                        dotNetRef.invokeMethodAsync('SearchAsync', term, page).then(success).catch(failure);
                    },
                    processResults: function (data, params) {
                        params.page = params.page || 1;
                        var results = data?.results || data?.Results || [];
                        var more = data?.pagination?.more ?? data?.More ?? false;
                        return { results: results, pagination: { more: more } };
                    }
                },
                templateResult: function (item) {
                    if (!item.id) return item.text;
                    var description = item.description || item.Description;
                    return description ? $('<span>').text(item.text).append($('<small class="d-block text-muted">').text(description)) : $('<span>').text(item.text);
                },
                escapeMarkup: function (markup) { return markup; }
            });

            $select.off('change.seafoodSelect2').on('change.seafoodSelect2', function () {
                var value = $(this).val();
                var text = value ? $(this).find('option:selected').text() : null;
                dotNetRef.invokeMethodAsync('OnSelectionChangeAsync', value ? String(value) : null, text).catch(function (error) {
                    console.error('Seafood Select2 selection update failed.', error);
                });
            });

            this.setSelection(selectId, initialItems || [], Boolean(options?.disabled ?? options?.Disabled));
        },

        fillOptions: function ($select, items) {
            (items || []).forEach(function (item) {
                var id = item.id ?? item.Id;
                if (!id || $select.find('option[value="' + id + '"]').length) return;
                var text = item.text ?? item.Text ?? ('#' + id);
                var option = new Option(text, id, true, true);
                $(option).attr('data-description', item.description ?? item.Description ?? '');
                $select.append(option);
            });
        },

        setSelection: function (selectId, items, disabled) {
            var $select = $('#' + selectId);
            if (!$select.length || !$.fn.select2) return;
            this.fillOptions($select, items || []);
            var id = items && items.length ? (items[0].id ?? items[0].Id) : null;
            $select.val(id ? String(id) : null).prop('disabled', Boolean(disabled)).trigger('change.select2');
        },

        destroy: function (selectId) {
            var $select = $('#' + selectId);
            if (!$select.length) return;
            $select.off('.seafoodSelect2');
            if ($select.hasClass('select2-hidden-accessible')) $select.select2('destroy');
        }
    };
})(window, window.jQuery);
