﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.DiaSymReader.PortablePdb
{
    // TODO: Copied from Microsoft.DiaSymReader.PortablePdb. Share.

    internal static class IMetadataImportExtensions
    {
        public static string GetQualifiedTypeName(this IMetadataImport importer, EntityHandle typeDefOrRef)
        {
            string qualifiedName;
            if (typeDefOrRef.Kind == HandleKind.TypeDefinition)
            {
                importer.GetTypeDefProps(MetadataTokens.GetToken(typeDefOrRef), out qualifiedName, out _, out _);
            }
            else if (typeDefOrRef.Kind == HandleKind.TypeReference)
            {
                importer.GetTypeRefProps(MetadataTokens.GetToken(typeDefOrRef), out _, out qualifiedName);
            }
            else
            {
                qualifiedName = null;
            }

            return qualifiedName;
        }

        // Doesn't handle nested types.
        public static string GetQualifiedTypeName(this MetadataReader reader, EntityHandle typeDefOrRef)
        {
            string qualifiedName;
            if (typeDefOrRef.Kind == HandleKind.TypeDefinition)
            {
                var typeDef = reader.GetTypeDefinition((TypeDefinitionHandle)typeDefOrRef);
                if (typeDef.Namespace.IsNil)
                {
                    return reader.GetString(typeDef.Name);
                }
                else
                {
                    return reader.GetString(typeDef.Namespace) + "." + reader.GetString(typeDef.Name);
                }

            }
            else if (typeDefOrRef.Kind == HandleKind.TypeReference)
            {
                var typeRef = reader.GetTypeReference((TypeReferenceHandle)typeDefOrRef);
                if (typeRef.Namespace.IsNil)
                {
                    return reader.GetString(typeRef.Name);
                }
                else
                {
                    return reader.GetString(typeRef.Namespace) + "." + reader.GetString(typeRef.Name);
                }
            }
            else
            {
                qualifiedName = null;
            }

            return qualifiedName;
        }

        public static unsafe void GetTypeDefProps(this IMetadataImport importer, int typeDefinition, out string qualifiedName, out TypeAttributes attributes, out int baseType)
        {
            Marshal.ThrowExceptionForHR(importer.GetTypeDefProps(typeDefinition, null, 0, out int bufferLength, null, null));

            var buffer = new StringBuilder(bufferLength);
            int baseTypeValue;
            TypeAttributes attributesValue;
            Marshal.ThrowExceptionForHR(importer.GetTypeDefProps(typeDefinition, buffer, buffer.Capacity, out bufferLength, &attributesValue, &baseTypeValue));

            qualifiedName = buffer.ToString();
            attributes = attributesValue;
            baseType = baseTypeValue;
        }

        public static unsafe void GetTypeRefProps(this IMetadataImport importer, int typeReference, out int resolutionScope, out string qualifiedName)
        {
            Marshal.ThrowExceptionForHR(importer.GetTypeRefProps(typeReference, null, null, 0, out int bufferLength));

            var buffer = new StringBuilder(bufferLength);
            int resolutionScopeValue;
            Marshal.ThrowExceptionForHR(importer.GetTypeRefProps(typeReference, &resolutionScopeValue, buffer, buffer.Capacity, out bufferLength));
            resolutionScope = resolutionScopeValue;
            qualifiedName = buffer.ToString();
        }
    }
}