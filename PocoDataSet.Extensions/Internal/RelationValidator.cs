using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.Extensions.Relations;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides RelationValidator functionality
    /// </summary>
    internal class RelationValidator
    {
        #region Public Methods
        public static void ValidateRelationDefinition(IDataSet dataSet, IDataRelation relation, RelationValidationOptions options, List<RelationIntegrityViolation> violations, out IDataTable? parentTable, out IDataTable? childTable)
        {
            parentTable = null;
            childTable = null;

            if (string.IsNullOrWhiteSpace(relation.RelationName))
            {
                // We still validate unnamed relation, but give it a placeholder.
                relation.RelationName = "<unnamed>";
            }

            // Check column counts
            if (relation.ParentColumns == null || relation.ChildColumns == null || relation.ParentColumns.Count == 0 || relation.ChildColumns.Count == 0)
            {
                if (options.ReportInvalidRelationDefinitions)
                {
                    violations.Add(new RelationIntegrityViolation
                    {
                        RelationName = relation.RelationName,
                        ParentTable = relation.ParentTable ?? string.Empty,
                        ChildTable = relation.ChildTable ?? string.Empty,
                        Kind = RelationIntegrityViolationKind.InvalidRelationDefinition,
                        Message = "Relation has no parent/child columns configured."
                    });
                }
                return;
            }

            if (relation.ParentColumns.Count != relation.ChildColumns.Count)
            {
                if (options.ReportInvalidRelationDefinitions)
                {
                    violations.Add(new RelationIntegrityViolation
                    {
                        RelationName = relation.RelationName,
                        ParentTable = relation.ParentTable ?? string.Empty,
                        ChildTable = relation.ChildTable ?? string.Empty,
                        Kind = RelationIntegrityViolationKind.InvalidRelationDefinition,
                        Message = "ParentColumns and ChildColumns count mismatch."
                    });
                }
                return;
            }

            // Tables
            if (!dataSet.TryGetTable(relation.ParentTable, out parentTable) || parentTable == null)
            {
                if (options.ReportInvalidRelationDefinitions)
                {
                    violations.Add(new RelationIntegrityViolation
                    {
                        RelationName = relation.RelationName,
                        ParentTable = relation.ParentTable ?? string.Empty,
                        ChildTable = relation.ChildTable ?? string.Empty,
                        Kind = RelationIntegrityViolationKind.InvalidRelationDefinition,
                        Message = "Parent table not found in DataSet."
                    });
                }
                parentTable = null;
                return;
            }

            if (!dataSet.TryGetTable(relation.ChildTable, out childTable) || childTable == null)
            {
                if (options.ReportInvalidRelationDefinitions)
                {
                    violations.Add(new RelationIntegrityViolation
                    {
                        RelationName = relation.RelationName,
                        ParentTable = relation.ParentTable ?? string.Empty,
                        ChildTable = relation.ChildTable ?? string.Empty,
                        Kind = RelationIntegrityViolationKind.InvalidRelationDefinition,
                        Message = "Child table not found in DataSet."
                    });
                }
                childTable = null;
                return;
            }

            // Columns existence
            if (!TableHasAllColumns(parentTable, relation.ParentColumns) || !TableHasAllColumns(childTable, relation.ChildColumns))
            {
                if (options.ReportInvalidRelationDefinitions)
                {
                    violations.Add(new RelationIntegrityViolation
                    {
                        RelationName = relation.RelationName,
                        ParentTable = relation.ParentTable ?? string.Empty,
                        ChildTable = relation.ChildTable ?? string.Empty,
                        Kind = RelationIntegrityViolationKind.InvalidRelationDefinition,
                        Message = "One or more columns referenced by the relation are missing in tables."
                    });
                }
                parentTable = null;
                childTable = null;
                return;
            }
        }

        public static bool TableHasAllColumns(IDataTable table, List<string> columnNames)
        {
            if (table.Columns == null || columnNames == null)
            {
                return false;
            }

            for (int i = 0; i < columnNames.Count; i++)
            {
                string columnName = columnNames[i];
                bool found = false;

                for (int j = 0; j < table.Columns.Count; j++)
                {
                    IColumnMetadata meta = table.Columns[j];
                    if (meta != null && string.Equals(meta.ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        public static void ValidateOrphanChildRows(
            IDataRelation relation,
            IDataTable parentTable,
            IDataTable childTable,
            RelationValidationOptions options,
            List<RelationIntegrityViolation> violations)
        {
            IReadOnlyList<IDataRow> childRows = childTable.Rows;
            if (childRows == null)
            {
                return;
            }

            for (int i = 0; i < childRows.Count; i++)
            {
                IDataRow childRow = childRows[i];
                if (childRow == null)
                {
                    continue;
                }

                if (options.IgnoreDeletedChildRows && childRow.DataRowState == DataRowState.Deleted)
                {
                    continue;
                }

                // Optional FK: treat nulls as "not set"
                if (options.TreatNullForeignKeysAsNotSet)
                {
                    bool anyNull = false;
                    for (int c = 0; c < relation.ChildColumns.Count; c++)
                    {
                        object? val = childRow[relation.ChildColumns[c]];
                        if (val == null)
                        {
                            anyNull = true;
                            break;
                        }
                    }

                    if (anyNull)
                    {
                        continue;
                    }
                }

                IDataRow? parentRow = FindParentRow(relation, parentTable, childRow, options);
                if (parentRow == null)
                {
                    RelationIntegrityViolation violation = new RelationIntegrityViolation
                    {
                        RelationName = relation.RelationName,
                        ParentTable = relation.ParentTable,
                        ChildTable = relation.ChildTable,
                        Kind = RelationIntegrityViolationKind.OrphanChildRow,
                        Message = "Child row references missing parent row."
                    };

                    CaptureKeySnapshot(relation.ChildColumns, childRow, violation.ChildKey);
                    CaptureKeySnapshotMapped(relation.ParentColumns, relation.ChildColumns, childRow, violation.ParentKey);

                    violations.Add(violation);
                }
            }
        }

        public static void ValidateDeletedParentsHaveNoChildren(
            IDataRelation relation,
            IDataTable parentTable,
            IDataTable childTable,
            RelationValidationOptions options,
            List<RelationIntegrityViolation> violations)
        {
            IReadOnlyList<IDataRow> parentRows = parentTable.Rows;
            if (parentRows == null)
            {
                return;
            }

            for (int i = 0; i < parentRows.Count; i++)
            {
                IDataRow parentRow = parentRows[i];
                if (parentRow == null)
                {
                    continue;
                }

                if (parentRow.DataRowState != DataRowState.Deleted)
                {
                    continue;
                }

                // If parent is deleted, check if any non-deleted child exists.
                bool hasChild = ParentHasAnyActiveChild(relation, parentRow, childTable, options);
                if (!hasChild)
                {
                    continue;
                }

                RelationIntegrityViolation violation = new RelationIntegrityViolation
                {
                    RelationName = relation.RelationName,
                    ParentTable = relation.ParentTable,
                    ChildTable = relation.ChildTable,
                    Kind = RelationIntegrityViolationKind.DeletedParentHasChildren,
                    Message = "Parent row is deleted but non-deleted child rows still exist (restrict delete)."
                };

                CaptureKeySnapshot(relation.ParentColumns, parentRow, violation.ParentKey);

                violations.Add(violation);
            }
        }

        public static bool ParentHasAnyActiveChild(
            IDataRelation relation,
            IDataRow parentRow,
            IDataTable childTable,
            RelationValidationOptions options)
        {
            IReadOnlyList<IDataRow> childRows = childTable.Rows;
            if (childRows == null)
            {
                return false;
            }

            for (int i = 0; i < childRows.Count; i++)
            {
                IDataRow childRow = childRows[i];
                if (childRow == null)
                {
                    continue;
                }

                if (options.IgnoreDeletedChildRows && childRow.DataRowState == DataRowState.Deleted)
                {
                    continue;
                }

                if (ChildMatchesParent(relation, parentRow, childRow))
                {
                    return true;
                }
            }

            return false;
        }

        public static IDataRow? FindParentRow(
            IDataRelation relation,
            IDataTable parentTable,
            IDataRow childRow,
            RelationValidationOptions options)
        {
            IReadOnlyList<IDataRow> parentRows = parentTable.Rows;
            if (parentRows == null)
            {
                return null;
            }

            for (int i = 0; i < parentRows.Count; i++)
            {
                IDataRow parentRow = parentRows[i];
                if (parentRow == null)
                {
                    continue;
                }

                if (options.TreatDeletedParentAsMissing && parentRow.DataRowState == DataRowState.Deleted)
                {
                    continue;
                }

                bool match = true;
                for (int c = 0; c < relation.ParentColumns.Count; c++)
                {
                    string parentCol = relation.ParentColumns[c];
                    string childCol = relation.ChildColumns[c];

                    object? parentVal = parentRow[parentCol];
                    object? childVal = childRow[childCol];

                    if (!object.Equals(parentVal, childVal))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return parentRow;
                }
            }

            return null;
        }

        public static bool ChildMatchesParent(IDataRelation relation, IDataRow parentRow, IDataRow childRow)
        {
            for (int c = 0; c < relation.ParentColumns.Count; c++)
            {
                string parentCol = relation.ParentColumns[c];
                string childCol = relation.ChildColumns[c];

                object? parentVal = parentRow[parentCol];
                object? childVal = childRow[childCol];

                if (!object.Equals(parentVal, childVal))
                {
                    return false;
                }
            }

            return true;
        }


        public static void CaptureKeySnapshot(List<string> columns, IDataRow row, Dictionary<string, object?> target)
        {
            if (columns == null || row == null || target == null)
            {
                return;
            }

            for (int i = 0; i < columns.Count; i++)
            {
                string col = columns[i];
                target[col] = row[col];
            }
        }

        // For orphan errors we want "expected parent key" derived from child's FK columns.
        public static void CaptureKeySnapshotMapped(
            List<string> parentColumns,
            List<string> childColumns,
            IDataRow childRow,
            Dictionary<string, object?> target)
        {
            if (parentColumns == null || childColumns == null || childRow == null || target == null)
            {
                return;
            }

            int count = parentColumns.Count;
            if (childColumns.Count < count)
            {
                count = childColumns.Count;
            }

            for (int i = 0; i < count; i++)
            {
                target[parentColumns[i]] = childRow[childColumns[i]];
            }
        }
        #endregion
    }
}
